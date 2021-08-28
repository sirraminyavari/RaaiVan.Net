using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web;
using System.Security.Cryptography;
using System.Drawing;
using System.Collections;

namespace RaaiVan.Modules.GlobalUtilities
{
    public enum FileOwnerTypes
    {
        None,
        Node,
        FormElement,
        Message,
        Wiki,
        WikiContent,
        WorkFlow,
        PDFCover
    }

    public enum FolderNames
    {
        TemporaryFiles,
        Attachments,
        ProfileImages,
        PDFImages,
        PDFCovers,
        HighQualityProfileImage,
        CoverPhoto,
        HighQualityCoverPhoto,
        Icons,
        HighQualityIcon,
        ApplicationIcons,
        HighQualityApplicationIcon,
        Index,
        Themes,
        WikiContent,
        Pictures,
        EmailTemplates
    }
    
    public enum DefaultIconTypes
    {
        None,
        Node,
        Document,
        Extension
    }

    public enum IconType {
        None,
        ProfileImage,
        CoverPhoto,
        Icon,
        ApplicationIcon
    }

    public class DocumentUtilities
    {
        private static byte[] _FavIcon;

        public static byte[] FavIcon
        {
            get
            {
                if (_FavIcon !=  null) return _FavIcon;

                try
                {
                    byte[] fav = File.ReadAllBytes(PublicMethods.map_path(PublicConsts.FavIcon));
                    if (!PublicMethods.is_dev()) _FavIcon = fav;
                    return fav;
                }
                catch { return new byte[0]; }
            }
        }

        private static Dictionary<string, string> _StaticFiles = new Dictionary<string, string>();

        public static string StaticFile(string name)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;

            if (_StaticFiles.ContainsKey(name.ToLower())) return _StaticFiles[name.ToLower()];

            try
            {
                string path = PublicMethods.map_path(name);
                string content = !File.Exists(path) ? string.Empty : File.ReadAllText(path);
                if (!PublicMethods.is_dev()) _StaticFiles[name.ToLower()] = content;
                return content;
            }
            catch { return string.Empty; }
        }

        protected static DocFileInfo _get_file_info(Dictionary<string, object> dic)
        {
            if (dic == null) return null;

            Guid? fileId = !dic.ContainsKey("FileID") ? null : PublicMethods.parse_guid(dic["FileID"].ToString());
            string extension = !dic.ContainsKey("Extension") ? null : PublicMethods.parse_string(dic["Extension"].ToString());
            string fileName = !dic.ContainsKey("FileName") ? null : PublicMethods.parse_string(dic["FileName"].ToString());
            long? size = !dic.ContainsKey("Size") ? null : PublicMethods.parse_long(dic["Size"].ToString());
            Guid? ownerId = !dic.ContainsKey("OwnerID") ? null : PublicMethods.parse_guid(dic["OwnerID"].ToString());

            FileOwnerTypes ownerType = FileOwnerTypes.None;
            if (dic.ContainsKey("OwnerType")) Enum.TryParse<FileOwnerTypes>(dic["OwnerType"].ToString(), true, out ownerType);

            DocFileInfo fi = new DocFileInfo()
            {
                FileID = fileId,
                FileName = fileName,
                Extension = extension,
                Size = size
            };

            if (ownerId.HasValue && ownerId != Guid.Empty) fi.OwnerID = ownerId;
            if (ownerType != FileOwnerTypes.None) fi.OwnerType = ownerType;

            return !fileId.HasValue ? null : fi;
        }

        public static List<DocFileInfo> get_files_info(string strFiles)
        {
            List<DocFileInfo> retList = new List<DocFileInfo>();

            if (string.IsNullOrEmpty(strFiles)) return retList;

            Dictionary<string, object> dic = PublicMethods.fromJSON("{\"Items\":" + Base64.decode(strFiles) + "}");

            if (!dic.ContainsKey("Items")) return retList;

            if (dic["Items"].GetType() == typeof(Dictionary<string, object>)) {
                DocFileInfo fi = _get_file_info((Dictionary<string, object>)dic["Items"]);
                if (fi != null) retList.Add(fi);
            }
            else if (dic["Items"].GetType() == typeof(ArrayList))
            {
                foreach (object obj in (ArrayList)dic["Items"])
                {
                    Dictionary<string, object> f = obj.GetType() == typeof(string) ? PublicMethods.fromJSON((string)obj) :
                        (obj.GetType() == typeof(Dictionary<string, object>) ? (Dictionary<string, object>)obj : null);

                    DocFileInfo fi = f == null ? null : _get_file_info(f);
                    if (fi != null) retList.Add(fi);
                }
            }

            return retList;
        }
        
        public static string get_files_json(Guid applicationId, List<DocFileInfo> attachedFiles, bool icon = false)
        {
            return attachedFiles == null || attachedFiles.Count == 0 ? "[]" : 
                "[" + string.Join(",", attachedFiles.Select(u => u.toJson(applicationId, icon))) + "]";
        }

        public static DocFileInfo decode_base64_file_content(Guid applicationId, Guid? ownerId,
            string base64FileContent, FileOwnerTypes ownerType)
        {
            if (string.IsNullOrEmpty(base64FileContent)) return null;

            byte[] theData = null;

            try { theData = Convert.FromBase64String(base64FileContent); }
            catch { return null; }

            int FIXED_HEADER = 16;

            DocFileInfo ret = new DocFileInfo()
            {
                FileID = Guid.NewGuid(),
                OwnerID = ownerId,
                OwnerType = ownerType,
                FolderName = FolderNames.TemporaryFiles
            };

            try
            {
                using (MemoryStream ms = new MemoryStream(theData))
                {
                    using (BinaryReader theReader = new BinaryReader(ms))
                    {
                        //Position the reader to get the file size.
                        byte[] headerData = new byte[FIXED_HEADER];
                        headerData = theReader.ReadBytes(headerData.Length);

                        ret.Size = (int)theReader.ReadUInt32();
                        int fileNameLength = (int)theReader.ReadUInt32() * 2;

                        if (fileNameLength <= 0 || fileNameLength > 255) throw new Exception("what the fuzz!!");

                        byte[] fileNameBytes = theReader.ReadBytes(fileNameLength);
                        //InfoPath uses UTF8 encoding.
                        Encoding enc = Encoding.Unicode;
                        string fullFileName = enc.GetString(fileNameBytes, 0, fileNameLength - 2);

                        int dotIndex = fullFileName.LastIndexOf(".");
                        if (dotIndex > 0 && dotIndex < (fullFileName.Length - 1))
                            ret.Extension = fullFileName.Substring(dotIndex + 1);

                        ret.FileName = string.IsNullOrEmpty(ret.Extension) ?
                            fullFileName : fullFileName.Substring(0, dotIndex);

                        byte[] fileBytes = theReader.ReadBytes((int)ret.Size.Value);

                        if (!ret.store(applicationId, fileBytes)) return null;
                    }
                }

                return ret;
            }
            catch (Exception ex)
            {
                //maybe the file is a base64 image!!
                try
                {
                    Image img = PublicMethods.image_from_byte_array(theData);
                    if (img == null) return null;
                    byte[] imageBytes = PublicMethods.image_to_byte_array(img, System.Drawing.Imaging.ImageFormat.Jpeg);
                    if (imageBytes == null || imageBytes.Length == 0) return null;

                    ret.Size = imageBytes.Length;
                    ret.FileName = "img";
                    ret.Extension = "jpg";

                    if (!ret.store(applicationId, imageBytes)) return null;

                    return ret;
                }
                catch { return null; }
            }
        }

        public static string get_personal_image_address(Guid? applicationId, 
            Guid? userId, bool networkAddress = false, bool highQuality = false)
        {
            if (RaaiVanSettings.SAASBasedMultiTenancy) applicationId = null;

            if (!userId.HasValue || userId == Guid.Empty)
            {
                string addr = PublicConsts.DefaultProfileImageURL;

                return highQuality ? string.Empty :
                    (networkAddress ? addr.Replace("../..", RaaiVanSettings.RaaiVanURL(applicationId)) : addr);
            }

            FolderNames folderName = highQuality ? FolderNames.HighQualityProfileImage : FolderNames.ProfileImages;

            DocFileInfo fi = new DocFileInfo() {
                FileID = userId,
                Extension = "jpg",
                FolderName = folderName
            };

            string address = !fi.exists(applicationId) ? 
                (highQuality ? string.Empty : PublicConsts.DefaultProfileImageURL) : fi.url(applicationId);

            return networkAddress ? address.Replace("../..", RaaiVanSettings.RaaiVanURL(applicationId)) : address;
        }
        
        public static bool picture_exists(Guid applicationId, Guid pictureId)
        {
            if (pictureId == Guid.Empty) return false;

            DocFileInfo fi = new DocFileInfo() {
                FileID = pictureId,
                Extension = "jpg",
                FolderName = FolderNames.Pictures
            };

            return fi.exists(applicationId);
        }

        private static bool get_icon_parameters(Guid? applicationId, IconType iconType, ref int width, ref int height, 
            ref int highQualityWidth, ref int highQualityHeight, ref FolderNames folder, 
            ref FolderNames highQualityFolder, ref string defaultIconUrl)
        {
            switch (iconType)
            {
                case IconType.ProfileImage:
                    width = height = 100;
                    highQualityWidth = highQualityHeight = 600;
                    folder = FolderNames.ProfileImages;
                    highQualityFolder = FolderNames.HighQualityProfileImage;
                    defaultIconUrl = get_personal_image_address(applicationId, null);
                    break;
                case IconType.CoverPhoto:
                    width = 900;
                    height = 220;
                    highQualityWidth = 1800;
                    highQualityHeight = 600;
                    folder = FolderNames.CoverPhoto;
                    highQualityFolder = FolderNames.HighQualityCoverPhoto;
                    defaultIconUrl = get_cover_photo_url(applicationId, null);
                    break;
                case IconType.Icon:
                    width = height = 100;
                    highQualityWidth = highQualityHeight = 600;
                    folder = FolderNames.Icons;
                    highQualityFolder = FolderNames.HighQualityIcon;
                    defaultIconUrl = get_icon_url(applicationId, DefaultIconTypes.Node);
                    break;
                case IconType.ApplicationIcon:
                    width = height = 100;
                    highQualityWidth = highQualityHeight = 600;
                    folder = FolderNames.ApplicationIcons;
                    highQualityFolder = FolderNames.HighQualityApplicationIcon;
                    defaultIconUrl = get_application_icon_url(null);
                    break;
                default:
                    return false;
            }

            return true;
        }

        public static bool get_icon_parameters(Guid? applicationId, IconType iconType, ref int width, ref int height, 
            ref int highQualityWidth, ref int highQualityHeight, ref FolderNames folder, ref FolderNames highQualityFolder) {
            string defaultIconUrl = string.Empty;

            return get_icon_parameters(applicationId, iconType, ref width, ref height, ref highQualityWidth, 
                ref highQualityHeight, ref folder, ref highQualityFolder, ref defaultIconUrl);
        }

        public static bool get_icon_parameters(Guid? applicationId, IconType iconType, ref int width, ref int height,
            ref FolderNames folder, ref FolderNames highQualityFolder)
        {
            int highQualityWidth = 0, highQualityHeight = 0;
            return get_icon_parameters(applicationId, iconType, ref width, ref height,
                ref highQualityWidth, ref highQualityHeight, ref folder, ref highQualityFolder);
        }

        public static bool get_icon_parameters(Guid? applicationId, IconType iconType, ref FolderNames folder, 
            ref FolderNames highQualityFolder, ref string defaultIconUrl)
        {
            int width = 0, height = 0, highQualityWidth = 0, highQualityHeight = 0;
            return get_icon_parameters(applicationId, iconType, ref width, ref height,
                ref highQualityWidth, ref highQualityHeight, ref folder, ref highQualityFolder, ref defaultIconUrl);
        }

        public static string get_icon_url(Guid? applicationId, DefaultIconTypes defaultIcon, 
            string extension = "", bool networkAddress = false)
        {
            string adr = string.Empty;

            switch (defaultIcon)
            {
                case DefaultIconTypes.Document:
                    adr = "../../images/archive.png";
                    break;
                case DefaultIconTypes.Extension:
                    adr = "../../images/extensions/" + extension + ".png";
                    string path = PublicMethods.map_path("~/images/extensions") + "\\" + extension + ".png";
                    adr = File.Exists(path) ? adr : "../../images/archive.png";
                    break;
                default:
                    adr = "../../images/Preview.png";
                    break;
            }

            return networkAddress ? adr.Replace("../..", RaaiVanSettings.RaaiVanURL(applicationId)) : adr;
        }
        
        public static string get_icon_url(Guid applicationId, Guid ownerId, 
            DefaultIconTypes defaultIcon = DefaultIconTypes.Node, Guid? alternateOwnerId = null, bool networkAddress = false)
        {
            if (ownerId == Guid.Empty) return string.Empty;

            DocFileInfo fi = new DocFileInfo() {
                FileID = ownerId,
                OwnerID = ownerId,
                Extension = "jpg",
                FolderName = FolderNames.Icons
            };

            string retUrl = fi.exists(applicationId) ? fi.url(applicationId) : string.Empty;

            if (string.IsNullOrEmpty(retUrl) && alternateOwnerId.HasValue)
            {
                fi.FileID = alternateOwnerId;
                retUrl = fi.exists(applicationId) ? fi.url(applicationId) : string.Empty;
            }

            if (string.IsNullOrEmpty(retUrl) && defaultIcon != DefaultIconTypes.None)
                retUrl = get_icon_url(applicationId, defaultIcon);

            return networkAddress ? retUrl.Replace("../..", RaaiVanSettings.RaaiVanURL(applicationId)) : retUrl;
        }
        
        public static string get_icon_url(Guid applicationId, Guid ownerId, string extension,
            bool highQuality = false, bool networkAddress = false)
        {
            if (ownerId == Guid.Empty) return string.Empty;

            FolderNames folderName = highQuality ? FolderNames.HighQualityIcon : FolderNames.Icons;

            DocFileInfo fi = new DocFileInfo()
            {
                FileID = ownerId,
                OwnerID = ownerId,
                Extension = "jpg",
                FolderName = folderName
            };

            string retUrl = fi.exists(applicationId) ? fi.url(applicationId) :
                (highQuality ? string.Empty : get_icon_url(applicationId, DefaultIconTypes.Extension, extension));

            return networkAddress ? retUrl.Replace("../..", RaaiVanSettings.RaaiVanURL(applicationId)) : retUrl;
        }
        
        public static string get_icon_url(Guid applicationId, string fileExtention, bool networkAddress = false)
        {
            string url = "~/images/extensions/" + fileExtention + ".png";
            string adr = File.Exists(PublicMethods.map_path(url)) ? url.Replace("~", "../..") : string.Empty;

            return networkAddress ? adr.Replace("../..", RaaiVanSettings.RaaiVanURL(applicationId)) : adr;
        }
        
        public static bool icon_exists(Guid applicationId, Guid ownerId)
        {
            if (ownerId == Guid.Empty) return false;

            DocFileInfo fi = new DocFileInfo() {
                FileID = ownerId,
                OwnerID = ownerId,
                Extension = "jpg",
                FolderName = FolderNames.Icons
            };

            return fi.exists(applicationId);
        }
        
        public static string get_icon_json(Guid applicationId, Guid ownerId)
        {
            return new DocFileInfo()
            {
                FileID = ownerId,
                FileName = "آیکون",
                Extension = "jpg",
                OwnerID = ownerId
            }.toJson(applicationId);
        }

        public static string get_application_icon_url(Guid? applicationId, bool highQuality = false, bool networkAddress = false)
        {
            FolderNames folderName = highQuality ? FolderNames.HighQualityApplicationIcon : FolderNames.ApplicationIcons;

            if (!applicationId.HasValue || applicationId == Guid.Empty)
            {
                string addr = RaaiVanSettings.LogoMiniURL;
                
                return highQuality ? string.Empty :
                    (networkAddress ? addr.Replace("../..", RaaiVanSettings.RaaiVanURL(applicationId)) : addr);
            }

            DocFileInfo fi = new DocFileInfo() {
                FileID = applicationId,
                OwnerID = applicationId,
                Extension = "jpg",
                FolderName = folderName
            };

            string retUrl = fi.exists(applicationId) ? fi.url(applicationId) : string.Empty;

            if (string.IsNullOrEmpty(retUrl) && !highQuality) retUrl = RaaiVanSettings.LogoMiniURL;

            return networkAddress ? retUrl.Replace("../..", RaaiVanSettings.RaaiVanURL(applicationId)) : retUrl;
        }

        public static string get_cover_photo_url(Guid? applicationId, 
            Guid? ownerId, bool networkAddress = false, bool highQuality = false)
        {
            if (RaaiVanSettings.SAASBasedMultiTenancy) applicationId = null;

            if (!ownerId.HasValue || ownerId == Guid.Empty)
            {
                string addr = PublicConsts.DefaultCoverPhotoURL;

                return highQuality ? string.Empty :
                    (networkAddress ? addr.Replace("../..", RaaiVanSettings.RaaiVanURL(applicationId)) : addr);
            }

            FolderNames folderName = highQuality ? FolderNames.HighQualityCoverPhoto : FolderNames.CoverPhoto;

            DocFileInfo fi = new DocFileInfo() {
                FileID = ownerId,
                OwnerID = ownerId,
                Extension = "jpg",
                FolderName = folderName
            };

            string retUrl = fi.exists(applicationId) ? fi.url(applicationId) : string.Empty;

            return networkAddress ? retUrl.Replace("../..", RaaiVanSettings.RaaiVanURL(applicationId)) : retUrl;
        }
        
        public static string get_download_url(Guid applicationId, Guid fileId)
        {
            return PublicConsts.get_complete_url(applicationId, PublicConsts.FileDownload) +
                "?timeStamp=" + DateTime.Now.Ticks.ToString() + "&FileID=" + fileId.ToString();
        }
        
        private static byte[] _aes_encryption(byte[] input, bool decrypt, bool useTokenKey)
        {
            //static key & salt
            byte[] AES_KEY = useTokenKey ? USBToken.read_encryption_key() : new byte[] {
                111, 14, 160, 236, 16, 107, 182, 80, 12, 58, 227, 77, 4, 127, 67, 27,
                212, 21, 173, 27, 254, 16, 130, 6, 198, 112, 21, 71, 144, 48, 170, 183
            };

            byte[] AES_SALT = new byte[] { 198, 254, 21, 67, 107, 14, 183, 80 };
            //end of static key & salt

            byte[] retBytes = new byte[0];

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (RijndaelManaged aes = new RijndaelManaged())
                {
                    aes.KeySize = 256;
                    aes.BlockSize = 128;
                    Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(AES_KEY, AES_SALT, 1000);
                    aes.Key = key.GetBytes(aes.KeySize / 8);
                    aes.IV = key.GetBytes(aes.BlockSize / 8);
                    aes.Mode = CipherMode.CBC;

                    using (CryptoStream cs = new CryptoStream(memoryStream,
                        decrypt ? aes.CreateDecryptor() : aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        try
                        {
                            cs.Write(input, 0, input.Length);
                            cs.Dispose();
                        }
                        catch { }
                    }

                    retBytes = memoryStream.ToArray();
                }
            }

            return retBytes;
        }

        private static byte[] _aes_encryption(byte[] input, bool decrypt) {
            return _aes_encryption(input, decrypt, RaaiVanSettings.USBToken);
        }

        public static byte[] encrypt_bytes_aes(byte[] input)
        {
            return _aes_encryption(input, decrypt: false);
        }

        public static byte[] encrypt_bytes_aes_native(byte[] input)
        {
            return _aes_encryption(input, decrypt: false, useTokenKey: false);
        }

        public static byte[] decrypt_bytes_aes(byte[] input)
        {
            return _aes_encryption(input, decrypt: true);
        }

        public static byte[] decrypt_bytes_aes_native(byte[] input)
        {
            return _aes_encryption(input, decrypt: true, useTokenKey: false);
        }
    }
    
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

        public DocFileInfo() {
            FolderName = null;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
        
        public string MIME() {
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

        public void refresh_folder_name() {
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

        private string get_address(Guid? applicationId, ref bool isPublic, bool? encrypted = null, bool ignoreExtension = false) {
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

            if (CephMode) return CephStorage.file_exists(address) ? address : string.Empty;
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

                string sourceAddress = get_real_address(applicationId);

                if (newGuidName.HasValue && newGuidName.Value != Guid.Empty) FileID = newGuidName;
                FolderName = destination;

                if (string.IsNullOrEmpty(sourceAddress))
                    return !string.IsNullOrEmpty(get_real_address(applicationId));

                if (CephMode)
                {
                    bool isPublic = false;
                    string newAddress = get_address(applicationId, ref isPublic, encrypted: is_encrypted(applicationId));
                    return CephStorage.rename_file(sourceAddress, newAddress, isPublic);
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

                if (CephMode) CephStorage.delete_file(address);
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

        public string get_text_content(Guid? applicationId) {
            byte[] content = toByteArray(applicationId);
            return content == null || content.Length == 0 ? string.Empty : Encoding.UTF8.GetString(content);
        }

        public string url(Guid? applicationId) {
            if (CephMode)
            {
                bool isPublic = false;
                string realAddress = get_real_address(applicationId, ref isPublic);
                return CephStorage.get_download_url(realAddress, isPublic);
            }
            else return "../../download/" + FileID.ToString() + 
                    (FolderName.HasValue ? "?Category=" + FolderName.ToString() : string.Empty);
        }

        public string toJson(Guid? applicationId, bool icon = false) {
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
