using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaaiVan.Modules.GlobalUtilities
{
    public class DocFileInfo : ICloneable
    {
        public Guid? OwnerID;
        private FileOwnerTypes _OwnerType;
        public Guid? FileID;
        public string FileName;
        public string Extension;
        public long? Size;
        public Guid? OwnerNodeID;
        public string OwnerNodeName;
        public string OwnerNodeType;
        public FolderNames? FolderName;
        private bool? _Encrypted;

        public FileOwnerTypes OwnerType
        {
            get { return _OwnerType; }

            set
            {
                _OwnerType = value;
                if (!FolderName.HasValue) FolderName = get_folder_name(value);
            }
        }

        public DocFileInfo()
        {
            FolderName = null;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public string MIME()
        {
            return PublicMethods.get_mime_type_by_extension(Extension);
        }

        public string file_name_with_extension
        {
            get
            {
                string fName = file_name_without_extension;

                return string.IsNullOrEmpty(fName) ? string.Empty :
                    fName + (string.IsNullOrEmpty(Extension) ? string.Empty : "." + Extension);
            }
        }

        public string file_name_without_extension
        {
            get
            {
                List<FolderNames> nameItems = new[] { FolderNames.EmailTemplates, FolderNames.PDFImages, FolderNames.Themes }.ToList();
                return nameItems.Any(n => FolderName == n) ? FileName : (FileID.HasValue ? FileID.ToString() : string.Empty);
            }
        }

        public void refresh_folder_name()
        {
            FolderName = get_folder_name(OwnerType);
        }

        private bool CephMode
        {
            get
            {
                return RaaiVanSettings.CephStorage.Enabled &&
                    !(new[] { FolderNames.Index, FolderNames.Themes }).Any(f => FolderName.HasValue && f == FolderName);
            }
        }

        private bool is_icon
        {
            get
            {
                List<FolderNames> iconFolders = new List<FolderNames>()
                {
                    FolderNames.ApplicationIcons,
                    FolderNames.HighQualityApplicationIcon,
                    FolderNames.Icons,
                    FolderNames.HighQualityIcon,
                    FolderNames.ProfileImages,
                    FolderNames.HighQualityProfileImage,
                    FolderNames.CoverPhoto,
                    FolderNames.HighQualityCoverPhoto,
                    FolderNames.Pictures
                };

                return FolderName.HasValue && iconFolders.Any(f => f == FolderName.Value);
            }
        }

        private string icon_redis_key
        {
            get
            {
                bool iconCheck = RedisAPI.Enabled && is_icon && FolderName.HasValue && FileID.HasValue;
                return !iconCheck ? string.Empty :
                    "exists_" + FolderName.ToString().ToLower() + "_" + FileID.ToString().ToLower();
            }
        }

        private bool is_encrypted(Guid? applicationId)
        {
            if (!_Encrypted.HasValue)
            {
                string normalAddress = get_address(applicationId, encrypted: false);
                string encryptedAddress = get_address(applicationId, encrypted: true);

                _Encrypted = !File.Exists(normalAddress) && File.Exists(encryptedAddress);
            }
            return _Encrypted.HasValue && _Encrypted.Value;
        }

        private static bool has_sub_folder(FolderNames folderName)
        {
            return !(new[] {
                FolderNames.EmailTemplates,
                FolderNames.Index,
                FolderNames.TemporaryFiles,
                FolderNames.Themes,
                FolderNames.EmailTemplates
            }).Any(f => folderName == f);
        }

        private static FolderNames get_folder_name(FileOwnerTypes ownerType)
        {
            switch (ownerType)
            {
                case FileOwnerTypes.Node:
                case FileOwnerTypes.Wiki:
                case FileOwnerTypes.Message:
                case FileOwnerTypes.WorkFlow:
                case FileOwnerTypes.FormElement:
                    return FolderNames.Attachments;
                case FileOwnerTypes.WikiContent:
                    return FolderNames.WikiContent;
                case FileOwnerTypes.PDFCover:
                    return FolderNames.PDFCovers;
                default:
                    return FolderNames.TemporaryFiles;
            }
        }

        private string _get_folder_path(Guid? applicationId, FolderNames folderName, ref bool isPublic, bool cephMode = false)
        {
            bool isAppLogo = folderName == FolderNames.ApplicationIcons || folderName == FolderNames.HighQualityApplicationIcon;
            bool isProfileImage = folderName == FolderNames.ProfileImages || folderName == FolderNames.HighQualityProfileImage;

            if (isAppLogo || (RaaiVanSettings.SAASBasedMultiTenancy && isProfileImage)) applicationId = null;

            string applicationPart = !applicationId.HasValue ? string.Empty : applicationId.Value.ToString() + "/";

            string mainFolder = string.Empty, contentFolder = string.Empty;
            isPublic = false;

            switch (folderName)
            {
                case FolderNames.Attachments:
                case FolderNames.WikiContent:
                case FolderNames.Index:
                case FolderNames.TemporaryFiles:
                case FolderNames.Pictures:
                case FolderNames.PDFImages:
                case FolderNames.PDFCovers:
                    mainFolder = "App_Data/";
                    contentFolder = applicationPart + (cephMode ? string.Empty : "Documents/") + folderName.ToString();
                    break;
                case FolderNames.Icons:
                case FolderNames.ApplicationIcons:
                case FolderNames.ProfileImages:
                case FolderNames.CoverPhoto:
                    mainFolder = "Global_Documents/";
                    contentFolder = applicationPart + folderName.ToString();
                    isPublic = true;
                    break;
                case FolderNames.HighQualityIcon:
                    mainFolder = "Global_Documents/";
                    contentFolder = applicationPart + FolderNames.Icons.ToString() + "/" + "HighQuality";
                    isPublic = true;
                    break;
                case FolderNames.HighQualityApplicationIcon:
                    mainFolder = "Global_Documents/";
                    contentFolder = applicationPart + FolderNames.ApplicationIcons.ToString() + "/" + "HighQuality";
                    isPublic = true;
                    break;
                case FolderNames.HighQualityProfileImage:
                    mainFolder = "Global_Documents/";
                    contentFolder = applicationPart + FolderNames.ProfileImages.ToString() + "/" + "HighQuality";
                    isPublic = true;
                    break;
                case FolderNames.HighQualityCoverPhoto:
                    mainFolder = "Global_Documents/";
                    contentFolder = applicationPart + FolderNames.CoverPhoto.ToString() + "/" + "HighQuality";
                    isPublic = true;
                    break;
                case FolderNames.Themes:
                    mainFolder = "CSS/";
                    contentFolder = folderName.ToString();
                    isPublic = true;
                    break;
                case FolderNames.EmailTemplates:
                    mainFolder = "App_Data/";
                    contentFolder = applicationPart + folderName.ToString();
                    break;
            }

            return (cephMode ? string.Empty : mainFolder) + contentFolder;
        }

        private string get_sub_folder()
        {
            if (!FileID.HasValue) return string.Empty;
            string str = FileID.ToString();
            return str[0].ToString() + str[1].ToString() + "\\" + str[2].ToString();
        }

        private string map_path(Guid? applicationId, ref bool isPublic, string dest = null)
        {
            if (!FolderName.HasValue) return string.Empty;

            dest = string.IsNullOrEmpty(dest) ? string.Empty : (dest[0] == '\\' ? string.Empty : "\\") + dest;

            string folder = _get_folder_path(applicationId, FolderName.Value, ref isPublic, cephMode: CephMode) + dest.Replace("\\", "/");

            return CephMode ? folder : PublicMethods.map_path("~/" + folder);
        }

        private string get_folder_address(Guid? applicationId, ref bool isPublic)
        {
            if (!FolderName.HasValue) return string.Empty;

            string sub = !has_sub_folder(FolderName.Value) ? string.Empty : "\\" + get_sub_folder();

            if (FolderName == FolderNames.PDFImages && FileID.HasValue)
                sub += "\\" + FileID.Value.ToString();

            return map_path(applicationId, ref isPublic, dest: sub);
        }

        private string get_folder_address(Guid? applicationId)
        {
            bool isPublic = false;
            return get_folder_address(applicationId, ref isPublic);
        }

        public static string index_folder_address(Guid? applicationId)
        {
            return new DocFileInfo() { FolderName = FolderNames.Index }.get_folder_address(applicationId);
        }

        public static string temporary_folder_address(Guid? applicationId)
        {
            return new DocFileInfo() { FolderName = FolderNames.TemporaryFiles }.get_folder_address(applicationId);
        }

        public int files_count_in_folder(Guid? applicationId)
        {
            try
            {
                string folderAddress = get_folder_address(applicationId);
                return CephMode ? CephStorage.files(folderAddress).Count : Directory.GetFiles(folderAddress).Length;
            }
            catch { return 0; }
        }

        public bool file_exists_in_folder(Guid? applicationId)
        {
            return CephMode ? CephStorage.folder_exists(get_folder_address(applicationId)) : files_count_in_folder(applicationId) > 0;
        }

        private string get_address(Guid? applicationId, ref bool isPublic, bool? encrypted = null, bool ignoreExtension = false)
        {
            string fileName = ignoreExtension ? file_name_without_extension : file_name_with_extension;

            if (string.IsNullOrEmpty(fileName)) return string.Empty;

            string encryptedPrefix = encrypted.HasValue && encrypted.Value ? PublicConsts.EncryptedFileNamePrefix : "";

            return get_folder_address(applicationId, ref isPublic) + (CephMode ? "/" : "\\") + encryptedPrefix + fileName;
        }

        private string get_address(Guid? applicationId, bool? encrypted = null, bool ignoreExtension = false)
        {
            bool isPublic = false;
            return get_address(applicationId, ref isPublic, encrypted: encrypted, ignoreExtension: ignoreExtension);
        }

        private string get_real_address(Guid? applicationId, ref bool isPublic)
        {
            _Encrypted = false;

            string folderPath = get_folder_address(applicationId, ref isPublic);

            if (string.IsNullOrEmpty(folderPath)) return string.Empty;

            string address = get_address(applicationId, encrypted: is_encrypted(applicationId));

            if (string.IsNullOrEmpty(address)) return string.Empty;

            string extLess = CephMode ? string.Empty :
                get_address(applicationId, encrypted: is_encrypted(applicationId), ignoreExtension: true);

            if (CephMode)
            {
                bool? fileExists = null;

                string redisKey = icon_redis_key;

                if (!string.IsNullOrEmpty(redisKey)) fileExists = RedisAPI.get_value<bool?>(redisKey);

                if (!fileExists.HasValue) fileExists = CephStorage.file_exists(address);

                if (!string.IsNullOrEmpty(redisKey) && fileExists.HasValue) RedisAPI.set_value<bool>(redisKey, fileExists.Value);

                return fileExists.HasValue && fileExists.Value ? address : string.Empty;
            }
            else return File.Exists(address) ? address :
               (!string.IsNullOrEmpty(extLess) && File.Exists(extLess) ? extLess : string.Empty);
        }

        private string get_real_address(Guid? applicationId)
        {
            bool isPublic = false;
            return get_real_address(applicationId, ref isPublic);
        }

        public bool store(Guid? applicationId, byte[] fileContent)
        {
            try
            {
                //Check for Encryption
                List<FolderNames> targetFolders =
                    new[] { FolderNames.TemporaryFiles, FolderNames.Attachments, FolderNames.WikiContent }.ToList();

                bool needsEncryption = targetFolders.Any(t => FolderName == t) &&
                    RaaiVanSettings.FileEncryption(applicationId) &&
                    ((Size.HasValue ? Size.Value : 0) / (1024 * 1024)) > 10;
                //end of Check for Encryption

                if (needsEncryption) fileContent = DocumentUtilities.encrypt_bytes_aes(fileContent);

                bool isPublic = false;
                string address = get_address(applicationId, ref isPublic, encrypted: needsEncryption);

                if (CephMode)
                {
                    if (!CephStorage.add_file(address, fileContent, isPublic)) return false;

                    if (!string.IsNullOrEmpty(icon_redis_key))
                        RedisAPI.set_value<bool>(icon_redis_key, true);
                }
                else
                {
                    string folderPath = get_folder_address(applicationId);
                    if (string.IsNullOrEmpty(folderPath)) return false;

                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                    using (FileStream fs = new FileStream(address, FileMode.Create))
                    using (BinaryWriter bw = new BinaryWriter(fs))
                        bw.Write(fileContent);
                }

                _Encrypted = needsEncryption;

                return true;
            }
            catch { return false; }
        }

        public bool move(Guid? applicationId, FolderNames source, FolderNames destination, Guid? newGuidName = null)
        {
            try
            {
                if (!FileID.HasValue) return false;

                FolderName = source;
                string sourceRedisKey = icon_redis_key;

                string sourceAddress = get_real_address(applicationId);

                if (newGuidName.HasValue && newGuidName.Value != Guid.Empty) FileID = newGuidName;
                FolderName = destination;
                string destinationRedisKey = icon_redis_key;

                if (string.IsNullOrEmpty(sourceAddress))
                    return !string.IsNullOrEmpty(get_real_address(applicationId));

                if (CephMode)
                {
                    bool isPublic = false;
                    string newAddress = get_address(applicationId, ref isPublic, encrypted: is_encrypted(applicationId));

                    bool result = CephStorage.rename_file(sourceAddress, newAddress, isPublic);

                    if (result && !string.IsNullOrEmpty(sourceRedisKey))
                        RedisAPI.set_value<bool>(sourceRedisKey, false);
                    else if (result && !string.IsNullOrEmpty(destinationRedisKey))
                        RedisAPI.set_value<bool>(destinationRedisKey, true);

                    return result;
                }
                else
                {
                    string destFolder = get_folder_address(applicationId);
                    string destinationAddress = get_address(applicationId, encrypted: is_encrypted(applicationId));

                    if (!Directory.Exists(destFolder)) Directory.CreateDirectory(destFolder);
                    File.Move(sourceAddress, destinationAddress);

                    return true;
                }
            }
            catch { return false; }
        }

        public bool exists(Guid? applicationId)
        {
            return !string.IsNullOrEmpty(get_real_address(applicationId));
        }

        public void delete(Guid? applicationId)
        {
            try
            {
                string address = get_real_address(applicationId);

                if (CephMode)
                {
                    if(CephStorage.delete_file(address) && !string.IsNullOrEmpty(icon_redis_key))
                        RedisAPI.set_value<bool>(icon_redis_key, false);
                }
                else if (File.Exists(address)) File.Delete(address);
            }
            catch { }
        }

        public bool readable(Guid applicationId)
        {
            return !string.IsNullOrEmpty(Extension) && ("jpg,png,jpeg,gif,bmp,mp4,mp3,wav,ogg,webm" +
                (RaaiVanConfig.Modules.PDFViewer(applicationId) ? ",pdf" : "")).Split(',').Any(x => x == Extension.ToLower());
        }

        public bool downloadable(Guid? applicationId)
        {
            return false;
            //string.IsNullOrEmpty(Extension) || !RaaiVanConfig.Modules.PDFViewer || Extension.ToLower() != "pdf";
        }

        public byte[] toByteArray(Guid? applicationId)
        {
            try
            {
                string fileAddress = get_real_address(applicationId);

                if (string.IsNullOrEmpty(fileAddress)) return new byte[0];
                else if (CephMode) return CephStorage.get_file(fileAddress);
                else return is_encrypted(applicationId) ?
                        DocumentUtilities.decrypt_bytes_aes(File.ReadAllBytes(fileAddress)) : File.ReadAllBytes(fileAddress);
            }
            catch { return new byte[0]; }
        }

        public string get_text_content(Guid? applicationId)
        {
            byte[] content = toByteArray(applicationId);
            return content == null || content.Length == 0 ? string.Empty : Encoding.UTF8.GetString(content);
        }

        public string url(Guid? applicationId)
        {
            if (CephMode)
            {
                bool isPublic = false;
                string realAddress = get_real_address(applicationId, ref isPublic);
                return CephStorage.get_download_url(realAddress, isPublic);
            }
            else return "../../download/" + FileID.ToString() +
                    (FolderName.HasValue ? "?Category=" + FolderName.ToString() : string.Empty);
        }

        public string toJson(Guid? applicationId, bool icon = false)
        {
            if (string.IsNullOrEmpty(FileName)) FileName = string.Empty;

            string iconName = string.IsNullOrEmpty(Extension) ? "dkgadjkghdkjghkfdjh" : Extension;

            string _path = PublicMethods.map_path("~/images/extensions/" + iconName + ".png");
            string _clPath = "../../images/extensions/" + iconName + ".png";

            return "{\"FileID\":\"" + FileID.Value.ToString() + "\"" +
                ",\"FileName\":\"" + Base64.encode(FileName) + "\"" +
                ",\"OwnerID\":\"" + (OwnerID.HasValue && OwnerID != Guid.Empty ? OwnerID.Value.ToString() : string.Empty) + "\"" +
                ",\"Extension\":\"" + Base64.encode(Extension) + "\",\"MIME\":\"" + MIME() + "\"" +
                ",\"Size\":" + (Size.HasValue ? Size.Value : 0).ToString() +
                ",\"Downloadable\":" + downloadable(applicationId).ToString().ToLower() +
                (!icon ? string.Empty :
                    ",\"IconURL\":\"" + (File.Exists(_path) ? _clPath : _clPath.Replace(iconName, "default")) + "\"") +
                "}";
        }
    }
}
