using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Amude.Domain;
using Amude.Domain.Attribute;
using Amude.Graphics;
using Amude.Global;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Amude.Motion;
using Microsoft.Xna.Framework;


namespace Amude.Core
{
    internal static class IO
    {
        const string AMUDE_CRYPTO_KEY = @"96aaa6dd-0d86-4305-83ec-8e38c06df1c1";
        const string TEXTS_PATH = "Content/Data/Global/Text/text.amude";
        const string MAP_DIR = "Content/Data/Map/";
        const string CHARACTER_DIR = "Content/Data/Character/";
        const string ENVIRONMENT_DIR = "Content/Data/Environment/";
        const string PROJECTILE_DIR = "Content/Data/Projectile/";
        const string SPECIALABILITY_DIR = "Content/Data/SpecialAbility/";
        const string TERRAIN_IMAGE_DIR = "Content/Image/Terrain/";
        const string AMUDE_EXTENSION = ".amude";
        private const int BUFFER_SIZE = 128 * 1024;
        private const ulong FC_TAG = 0xFC010203040506CF;

        private static ContentManager Content;

        public static void Initialize(ContentManager content)
        {
            IO.Content = content;
        }

        public static void VerifyConfigFiles()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(
                ConfigurationUserLevel.None);

            if (!config.HasFile)
                config.Save(ConfigurationSaveMode.Minimal, true);
            ConfigurationManager.RefreshSection("appSettings");
        }

        public static void WriteConfig(string key, string value)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(
                ConfigurationUserLevel.None);
            config.AppSettings.Settings.Remove(key);
            config.AppSettings.Settings.Add(key, value);
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        public static Dictionary<String, String> LoadTexts(ProgressBarCallBack callback)
        {
            Dictionary<String, String> texts = new Dictionary<string, string>();
            string fileContent = DecryptFile(TEXTS_PATH, AMUDE_CRYPTO_KEY);
            XmlDocument doc = new XmlDocument();
            doc.Load(XmlReader.Create(new StringReader(fileContent)));

            XmlElement mapElement = doc.DocumentElement;
            foreach (XmlNode text in mapElement.GetElementsByTagName("text"))
            {
                String key = text.Attributes["key"].InnerText;
                StringBuilder content = new StringBuilder();

                foreach (XmlNode paragraph in text.ChildNodes)
                {
                    content.AppendLine(paragraph.InnerText);
                }

                content.Replace("\r", "");
                texts.Add(key, content.ToString());
            }

            callback(Constants.CONFIG_FILES_COUNT);
            return texts;
        }

        public static SpriteFont LoadFont(string path)
        {
            return Content.Load<SpriteFont>(path);
        }

        public static List<Texture2D> LoadSprite(string path, int frames)
        {
            List<Texture2D> sprites = new List<Texture2D>();

            for (int i = 1; i <= frames; i++)
            {
                sprites.Add(Content.Load<Texture2D>(path + i.ToString("00")));
            }
            return sprites;
        }

        public static Texture2D LoadSingleTexture(string path)
        {
            return Content.Load<Texture2D>(path);
        }

        public static Map LoadMap(string mapName)
        {
            StringBuilder path = new StringBuilder();
            path.Append(Environment.CurrentDirectory);
            path.Append("/");
            path.Append(MAP_DIR);
            path.Append(mapName);
            path.Append(AMUDE_EXTENSION);

            string fileContent = DecryptFile(path.ToString(), AMUDE_CRYPTO_KEY);
            XmlDocument doc = new XmlDocument();
            doc.Load(XmlReader.Create(new StringReader(fileContent)));
            return ReadMap(doc);
        }

        public static Dictionary<String, Terrain> LoadTerrains()
        {
            Dictionary<String, Terrain> terrains = new Dictionary<String, Terrain>();
            DirectoryInfo dir = new DirectoryInfo(Environment.CurrentDirectory + "/" + TERRAIN_IMAGE_DIR);

            foreach (FileInfo terrainFile in dir.GetFiles())
            {
                string fileName = terrainFile.Name.Remove(terrainFile.Name.Length - 4);
                Terrain terrain = new Terrain(LoadSingleTexture(TERRAIN_IMAGE_DIR.Replace("Content/", "") + fileName));
                terrain.RootName = fileName;
                terrains.Add(terrain.RootName, terrain);
            }

            return terrains;
        }

        public static Dictionary<String, Character> LoadCharacters(ProgressBarCallBack callback)
        {
            Dictionary<String, Character> characters = new Dictionary<String, Character>();
            DirectoryInfo dir = new DirectoryInfo(Environment.CurrentDirectory + "/" + CHARACTER_DIR);

            foreach (FileInfo fileInfo in dir.GetFiles())
            {
                string fileContent = DecryptFile(fileInfo.FullName, AMUDE_CRYPTO_KEY);
                XmlDocument doc = new XmlDocument();

                doc.Load(XmlReader.Create(new StringReader(fileContent)));
                Character character = LoadCharacter(doc);
                characters.Add(character.RootName, character);
                callback(Constants.CONFIG_FILES_COUNT);
            }

            return characters;
        }

        public static Dictionary<String, Entity> LoadEnvironment(ProgressBarCallBack callback)
        {
            return LoadEntities(ENVIRONMENT_DIR, callback);
        }

        public static Dictionary<String, Entity> LoadProjectiles(ProgressBarCallBack callback)
        {
            return LoadEntities(PROJECTILE_DIR, callback);
        }

        public static Dictionary<SpecialAbility, Animation> LoadSpecialAbilities(ProgressBarCallBack callback)
        {
            Dictionary<SpecialAbility, Animation> specialAbilities = new Dictionary<SpecialAbility, Animation>();
            DirectoryInfo dir = new DirectoryInfo(Environment.CurrentDirectory + "/" + SPECIALABILITY_DIR);

            foreach (FileInfo fileInfo in dir.GetFiles())
            {
                string fileContent = DecryptFile(fileInfo.FullName, AMUDE_CRYPTO_KEY);
                XmlDocument doc = new XmlDocument();

                doc.Load(XmlReader.Create(new StringReader(fileContent)));

                KeyValuePair<SpecialAbility, Animation> kv = LoadSpecialAbility(doc);
                specialAbilities.Add(kv.Key, kv.Value);

                callback(Constants.CONFIG_FILES_COUNT);
            }

            return specialAbilities;
        }

        private static Dictionary<String, Entity> LoadEntities(String path, ProgressBarCallBack callback)
        {
            Dictionary<String, Entity> entities = new Dictionary<String, Entity>();
            DirectoryInfo dir = new DirectoryInfo(Environment.CurrentDirectory + "/" + path);

            foreach (FileInfo fileInfo in dir.GetFiles())
            {
                string fileContent = DecryptFile(fileInfo.FullName, AMUDE_CRYPTO_KEY);
                XmlDocument doc = new XmlDocument();

                doc.Load(XmlReader.Create(new StringReader(fileContent)));
                Entity entity = LoadEntity(doc);
                entities.Add(entity.RootName, entity);
                callback(Constants.CONFIG_FILES_COUNT);
            }

            return entities;
        }

        #region Decrypt

        private static string DecryptFile(string inFile, string password)
        {
            using (FileStream fin = File.OpenRead(inFile))
            {
                MemoryStream content = new MemoryStream();
                byte[] bytes = new byte[BUFFER_SIZE];
                int read = -1;
                int outValue = 0;

                // read off the IV and Salt
                byte[] IV = new byte[16];
                fin.Read(IV, 0, 16);
                byte[] salt = new byte[16];
                fin.Read(salt, 0, 16);

                SymmetricAlgorithm sma = CreateRijndael(password, salt);
                sma.IV = IV;

                long lSize = -1;

                HashAlgorithm hasher = SHA256.Create();

                using (CryptoStream cin = new CryptoStream(fin, sma.CreateDecryptor(), CryptoStreamMode.Read),
                          chash = new CryptoStream(Stream.Null, hasher, CryptoStreamMode.Write))
                {
                    BinaryReader br = new BinaryReader(cin);
                    lSize = br.ReadInt64();
                    ulong tag = br.ReadUInt64();

                    if (FC_TAG != tag)
                        throw new Exception("File Corrupted!");

                    long numReads = lSize / BUFFER_SIZE;

                    long slack = (long)lSize % BUFFER_SIZE;

                    for (int i = 0; i < numReads; ++i)
                    {
                        read = cin.Read(bytes, 0, bytes.Length);
                        content.Write(bytes, 0, read);
                        chash.Write(bytes, 0, read);
                        outValue += read;
                    }

                    if (slack > 0)
                    {
                        read = cin.Read(bytes, 0, (int)slack);
                        content.Write(bytes, 0, read);
                        chash.Write(bytes, 0, read);
                        outValue += read;
                    }

                    chash.Flush();
                    chash.Close();
                    content.Flush();

                    byte[] curHash = hasher.Hash;

                    byte[] oldHash = new byte[hasher.HashSize / 8];
                    read = cin.Read(oldHash, 0, oldHash.Length);
                    if ((oldHash.Length != read) || (!CheckByteArrays(oldHash, curHash)))
                        throw new Exception("File Corrupted!");
                }

                if (outValue != lSize)
                    throw new Exception("File Sizes don't match!");

                content.Position = 0;
                StreamReader reader = new StreamReader(content);
                return reader.ReadToEnd();
            }
        }

        private static SymmetricAlgorithm CreateRijndael(string password, byte[] salt)
        {
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(password, salt, "SHA256", 1000);

            SymmetricAlgorithm sma = Rijndael.Create();
            sma.KeySize = 256;
            sma.Key = pdb.GetBytes(32);
            sma.Padding = PaddingMode.PKCS7;
            return sma;
        }

        private static bool CheckByteArrays(byte[] b1, byte[] b2)
        {
            if (b1.Length == b2.Length)
            {
                for (int i = 0; i < b1.Length; ++i)
                {
                    if (b1[i] != b2[i])
                        return false;
                }
                return true;
            }
            return false;
        }

        #endregion

        #region XML Readers

        private static Entity LoadEntity(XmlDocument document)
        {
            Entity entity = new Entity();
            XmlElement entityElement = document.DocumentElement;
            XmlNode animationsNode = entityElement.GetElementsByTagName("animations")[0];
            string imageRootDir;
            int defaultAnimationFrames;

            entity.RootName = entityElement.GetElementsByTagName("rootName")[0].InnerText;
            entity.Name = entityElement.GetElementsByTagName("name")[0].InnerText;
            imageRootDir = entityElement.GetElementsByTagName("imageRootDirectory")[0].InnerText;
            defaultAnimationFrames = int.Parse(entityElement.GetElementsByTagName("defaultAnimationFrames")[0].InnerText);
            entity.DefaultSpeed = float.Parse(entityElement.GetElementsByTagName("defaultVelocity")[0].InnerText);

            LoadAnimations(entityElement.GetElementsByTagName("animations")[0], imageRootDir,
                defaultAnimationFrames, entity);

            return entity;
        }


        private static Character LoadCharacter(XmlDocument document)
        {
            Character character = new Character();
            XmlElement entity = document.DocumentElement;
            XmlNode characterNode = entity.GetElementsByTagName("character")[0];
            XmlNode animationsNode = entity.GetElementsByTagName("animations")[0];
            string imageRootDir;
            int defaultAnimationFrames;

            character.RootName = entity.GetElementsByTagName("rootName")[0].InnerText;
            character.Name = entity.GetElementsByTagName("name")[0].InnerText;
            imageRootDir = entity.GetElementsByTagName("imageRootDirectory")[0].InnerText;
            defaultAnimationFrames = int.Parse(entity.GetElementsByTagName("defaultAnimationFrames")[0].InnerText);
            character.DefaultSpeed = float.Parse(entity.GetElementsByTagName("defaultVelocity")[0].InnerText);

            if (characterNode.Attributes["specialAbility"] != null)
            {
                character.SpecialAbility = Bundle.GetSpecialAbility(characterNode.Attributes["specialAbility"].InnerText);
            }
            character.Description = characterNode.SelectSingleNode("description").InnerText;

            character.Health = new Health(float.Parse(characterNode.SelectSingleNode("health").InnerText));

            XmlNode attackNode = characterNode.SelectSingleNode("attack");
            AttackType attackType = (AttackType)Enum.Parse(typeof(AttackType),
                attackNode.Attributes["type"].InnerText.ToUpper());
            int attackRange = 1;
            if (attackNode.Attributes["range"] != null)
            {
                attackRange = int.Parse(attackNode.Attributes["range"].InnerText);
            }
            character.Attack = new Attack(attackType,
                float.Parse(attackNode.Attributes["min"].InnerText),
                float.Parse(attackNode.Attributes["max"].InnerText),
                attackRange);

            XmlNode defenseNode = characterNode.SelectSingleNode("defense");
            character.Defense = new Defense(
                float.Parse(defenseNode.Attributes["min"].InnerText),
                float.Parse(defenseNode.Attributes["max"].InnerText));

            character.Agility = int.Parse(characterNode.SelectSingleNode("agility").InnerText);

            character.Initiative = float.Parse(characterNode.SelectSingleNode("initiative").InnerText);

            character.ProjectileVelocity = 0;
            XmlNode projectileVelocityNode = characterNode.SelectSingleNode("projectileVelocity");
            if (projectileVelocityNode != null)
            {
                character.ProjectileVelocity = float.Parse(projectileVelocityNode.InnerText);
            }

            LoadAnimations(entity.GetElementsByTagName("animations")[0], imageRootDir,
                defaultAnimationFrames, character);

            return character;
        }

        private static KeyValuePair<SpecialAbility, Animation> LoadSpecialAbility(XmlDocument document)
        {
            SpecialAbility specialAbility = new SpecialAbility();
            Animation specialAbilityAnimation;
            String imageRootDir;
            XmlElement mainElement = document.DocumentElement;
            XmlNode effectNode = mainElement.GetElementsByTagName("effect")[0];
            XmlNode animationNode = mainElement.GetElementsByTagName("animation")[0];

            specialAbility.Type = (SpecialAbilityType)Enum.Parse(typeof(SpecialAbilityType),
                mainElement.Attributes["type"].InnerText);

            specialAbility.Friendly = bool.Parse(mainElement.Attributes["friendly"].InnerText);

            specialAbility.Name = mainElement.GetElementsByTagName("name")[0].InnerText;
            specialAbility.RootName = mainElement.GetElementsByTagName("rootName")[0].InnerText;
            imageRootDir = mainElement.GetElementsByTagName("imageRootDirectory")[0].InnerText;

            specialAbility.Percentage = 1;
            if (mainElement.GetElementsByTagName("percentage").Count > 0)
            {
                specialAbility.Percentage = float.Parse(mainElement.GetElementsByTagName("percentage")[0].InnerText);
            }

            specialAbility.Range = 1;
            if (specialAbility.Type == SpecialAbilityType.Passive)
            {
                specialAbility.Range = int.Parse(mainElement.GetElementsByTagName("range")[0].InnerText);
            }            

            specialAbility.Affect = new Affect();

            specialAbility.Affect.RootName = specialAbility.RootName;
            specialAbility.Affect.Health = 0;
            if (effectNode.SelectSingleNode("health") != null)
            {
                specialAbility.Affect.Health = float.Parse(effectNode.SelectSingleNode("health").InnerText);
            }

            specialAbility.Affect.Attack = 0;
            if (effectNode.SelectSingleNode("attack") != null)
            {
                specialAbility.Affect.Attack = int.Parse(effectNode.SelectSingleNode("attack").InnerText);
            }

            specialAbility.Affect.Defense = 0;
            if (effectNode.SelectSingleNode("defense") != null)
            {
                specialAbility.Affect.Defense = int.Parse(effectNode.SelectSingleNode("defense").InnerText);
            }

            specialAbility.Affect.Agility = 0;
            if (effectNode.SelectSingleNode("agility") != null)
            {
                specialAbility.Affect.Agility = int.Parse(effectNode.SelectSingleNode("agility").InnerText);
            }

            specialAbility.Affect.Initiative = 0;
            if (effectNode.SelectSingleNode("initiative") != null)
            {
                specialAbility.Affect.Initiative = int.Parse(effectNode.SelectSingleNode("initiative").InnerText);
            }

            specialAbility.Affect.Duration = 1;
            if (effectNode.SelectSingleNode("duration") != null)
            {
                specialAbility.Affect.Duration = int.Parse(effectNode.SelectSingleNode("duration").InnerText);
            }

            int frames = int.Parse(animationNode.Attributes["frames"].InnerText);

            AnimationType animationType = (AnimationType)Enum.Parse(typeof(AnimationType),
                animationNode.Attributes["type"].InnerText);

            List<Texture2D> sprites = LoadSprite(imageRootDir + animationType.ToString().ToLower() + "-", frames);
            
            float duration = float.Parse(animationNode.Attributes["duration"].InnerText);

            specialAbilityAnimation = new Animation(animationType, duration, sprites);

            if (animationNode.HasChildNodes)
            {
                XmlNode soundNode = animationNode.SelectSingleNode("sound");
                if (soundNode != null)
                {
                    specialAbilityAnimation.SetSound(specialAbility.RootName + "_" + soundNode.Attributes["name"].InnerText,
                        bool.Parse(soundNode.Attributes["isCyclic"].InnerText));
                }
            }

            return new KeyValuePair<SpecialAbility, Animation>(specialAbility, specialAbilityAnimation);
        }

        private static void LoadAnimations(XmlNode animations, string imageRootDir,
            int defaultAnimationFrames, Entity target)
        {
            foreach (XmlNode animationNode in animations.ChildNodes)
            {
                int frames = defaultAnimationFrames;
                if (animationNode.Attributes["frames"] != null)
                {
                    frames = int.Parse(animationNode.Attributes["frames"].InnerText);
                }

                AnimationType animationKey = (AnimationType)Enum.Parse(typeof(AnimationType),
                animationNode.Attributes["type"].InnerText);

                List<Texture2D> sprites = LoadSprite(imageRootDir + animationKey.ToString().ToLower() + "-", frames);

                float duration = float.Parse(animationNode.Attributes["duration"].InnerText);

                Animation animation = new Animation(animationKey, duration, sprites);
                animation.IsCyclic = bool.Parse(animationNode.Attributes["isCyclic"].InnerText);

                if (animationNode.Attributes["movementBehavior"] != null)
                {
                    animation.MovementBehavior = (MovementBehavior)Enum.Parse(typeof(MovementBehavior),
                            animationNode.Attributes["movementBehavior"].InnerText);
                }

                animation.Ready = Animation.INFINITY;
                if (animationNode.Attributes["ready"] != null)
                {
                    animation.Ready = int.Parse(animationNode.Attributes["ready"].InnerText);
                }

                animation.OnFinalize = OnFinalize.DoNothing;
                if (animationNode.Attributes["onFinalize"] != null)
                {
                    animation.OnFinalize = (OnFinalize)Enum.Parse(typeof(OnFinalize), 
                        animationNode.Attributes["onFinalize"].InnerText);
                }

                if (animationNode.HasChildNodes)
                {
                    XmlNode soundNode = animationNode.SelectSingleNode("sound");
                    XmlNode childAnimation = animationNode.SelectSingleNode("external");


                    if (soundNode != null)
                    {
                        animation.SetSound(target.RootName + "_" + soundNode.Attributes["name"].InnerText,
                            bool.Parse(soundNode.Attributes["isCyclic"].InnerText));
                    }

                    string childClass;
                    string childName;
                    AnimationType childType;


                    if (childAnimation != null)
                    {
                        childClass = childAnimation.Attributes["class"].InnerText;
                        childName = childAnimation.Attributes["rootName"].InnerText;
                        childType = (AnimationType)Enum.Parse(typeof(AnimationType),
                            childAnimation.Attributes["animationType"].InnerText);
                        animation.SetChild(Bundle.GetAnimation(childClass, childName, childType));
                    }
                    else
                    {
                        childAnimation = animationNode.SelectSingleNode("internal");
                        if (childAnimation != null)
                        {
                            childType = (AnimationType)Enum.Parse(typeof(AnimationType),
                                childAnimation.Attributes["animationType"].InnerText);
                            animation.SetChild(target.GetDefinedAnimation(childType));
                        }
                    }
                }

                target.DefineAnimatedMovement(animationKey, animation);
            }
        }

        private static Map ReadMap(XmlDocument document)
        {
            Map map;
            XmlElement mapElement = document.DocumentElement;
            string terrainMap = mapElement.GetElementsByTagName("terrain")[0].InnerText;
            string objectMap = mapElement.GetElementsByTagName("entities")[0].InnerText;
            int width = int.Parse(mapElement.Attributes["width"].InnerText);
            int height = int.Parse(mapElement.Attributes["height"].InnerText);
            map = new Map(width, height);

            XmlNodeList imports = mapElement.GetElementsByTagName("external");
            Dictionary<String, Terrain> importedTerrains = new Dictionary<String, Terrain>();
            Dictionary<String, Entity> importedObjects = new Dictionary<string, Entity>();

            foreach (XmlNode import in imports)
            {
                string importClass = import.Attributes["class"].InnerText;
                string importName = import.Attributes["rootName"].InnerText;
                string alias = null;
                if (import.Attributes["alias"] != null)
                {
                    alias = import.Attributes["alias"].InnerText;
                }

                switch (importClass)
                {
                    case "Terrains":
                        Terrain terrain = Bundle.Terrains[importName];
                        importedTerrains.Add(importName, terrain);
                        if (!string.IsNullOrEmpty(alias))
                        {
                            importedTerrains.Add(alias, terrain);
                        }
                        break;
                    case "Environment":
                        Entity environment = Bundle.Environment[importName];
                        importedObjects.Add(importName, environment);
                        if (!string.IsNullOrEmpty(alias))
                        {
                            importedObjects.Add(alias, environment);
                        }
                        break;
                }
            }

            terrainMap = terrainMap.Replace(" ", "").Replace("\n", "");
            String[] cells = terrainMap.Split(new String[] { "[", "]" }, StringSplitOptions.RemoveEmptyEntries);
            if (cells.Length != width * height)
            {
                throw new Exception("Corrupted File");
            }

            for (int i = 0; i < width * height; i++)
            {
                int x = i % width;
                int y = i / width;
                Terrain terrain = importedTerrains[cells[i]].Clone();
                terrain.MapLocation = new Point(x,y);
                map.Terrain[x, y] = terrain;
            }

            objectMap = objectMap.Replace(" ", "").Replace("\n", "");
            cells = objectMap.Split(new String[] { "[", "]" }, StringSplitOptions.RemoveEmptyEntries);
            if (cells.Length != width * height)
            {
                throw new Exception("Corrupted File");
            }
            for (int i = 0; i < width * height; i++)
            {
                if (string.IsNullOrEmpty(cells[i]) || cells[i] == ".")
                {
                    continue;
                }
                int x = i % width;
                int y = i / width;
                Entity entity = importedObjects[cells[i]].Clone();
                entity.LayerDepth = Constants.LD_ENVIRONMENT;
                entity.MapLocation = new Point(x, y);
                entity.AddAnimatedMovement(AnimationType.StaticRight);
                map.SetObject(entity);
            }

            return map;
        }

        #endregion
    }
}
