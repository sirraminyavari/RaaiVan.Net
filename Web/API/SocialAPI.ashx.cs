using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RaaiVan.Modules.Sharing;
using RaaiVan.Modules.Users;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.NotificationCenter;
using RaaiVan.Modules.CoreNetwork;
using RaaiVan.Modules.WorkFlow;
using RaaiVan.Modules.Log;
using RaaiVan.Modules.Privacy;

namespace RaaiVan.Web.API
{
    /// <summary>
    /// Summary description for SocialAPI
    /// </summary>
    public class SocialAPI : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        private ParamsContainer paramsContainer = null;

        public void ProcessRequest(HttpContext context)
        {
            paramsContainer = new ParamsContainer(context, nullTenantResponse: true);
            if (!paramsContainer.ApplicationID.HasValue) return;

            string responseText = string.Empty;
            string command = PublicMethods.parse_string(context.Request.Params["Command"], false);

            switch (command)
            {
                case "AddPost":
                    add_post(SharingUtilities.get_post_type_id(PublicMethods.parse_string(context.Request.Params["PostType"], false)),
                        PublicMethods.parse_string(context.Request.Params["Description"]),
                        PublicMethods.parse_guid(context.Request.Params["ObjectID"]), DateTime.Now,
                        PublicMethods.parse_guid(context.Request.Params["OwnerID"]),
                        PublicMethods.parse_string(context.Request.Params["OwnerType"], false),
                        SharingUtilities.get_privacy_type(PublicMethods.parse_string(context.Request.Params["Privacy"], false)),
                        DocumentUtilities.get_files_info(context.Request.Params["AttachedFile"]).FirstOrDefault(),
                        ref responseText);
                    _return_response(ref responseText);
                    return;
                case "UpdatePost":
                    update_post(PublicMethods.parse_guid(context.Request.Params["PostID"]),
                        PublicMethods.parse_string(context.Request.Params["Description"]), DateTime.Now, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "RemovePost":
                    remove_post(PublicMethods.parse_guid(context.Request.Params["PostID"]), ref responseText);
                    _return_response(ref responseText);
                    return;
                case "GetPost":
                    get_post(PublicMethods.parse_guid(context.Request.Params["PostID"]),
                        PublicMethods.parse_bool(context.Request.Params["OwnerInfo"]), ref responseText);
                    _return_response(ref responseText);
                    return;
                case "GetPosts":
                    get_posts(PublicMethods.parse_guid(context.Request.Params["OwnerID"]),
                        PublicMethods.parse_bool(context.Request.Params["OwnerInfo"]),
                        PublicMethods.parse_bool(context.Request.Params["News"]),
                        PublicMethods.parse_date(context.Request.Params["MaxDate"]),
                        PublicMethods.parse_date(context.Request.Params["MinDate"]),
                        PublicMethods.parse_int(context.Request.Params["Count"], 20), ref responseText);
                    _return_response(ref responseText);
                    return;
                case "Share":
                    share(PublicMethods.parse_guid(context.Request.Params["PostID"]),
                        PublicMethods.parse_guid(context.Request.Params["OwnerID"]),
                        PublicMethods.parse_string(context.Request.Params["Description"]), DateTime.Now,
                        SharingUtilities.get_privacy_type(PublicMethods.parse_string(context.Request.Params["Privacy"], false)),
                        PublicMethods.parse_string(context.Request.Params["OwnerType"], false), ref responseText);
                    _return_response(ref responseText);
                    return;
                case "AddComment":
                    add_comment(PublicMethods.parse_guid(context.Request.Params["PostID"]),
                        PublicMethods.parse_string(context.Request.Params["Description"]), DateTime.Now, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "ModifyComment":
                    update_comment(PublicMethods.parse_guid(context.Request.Params["CommentID"]),
                        PublicMethods.parse_string(context.Request.Params["Description"]), DateTime.Now, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "RemoveComment":
                    remove_comment(PublicMethods.parse_guid(context.Request.Params["CommentID"]), ref responseText);
                    _return_response(ref responseText);
                    return;
                case "GetComments":
                    get_comments(PublicMethods.parse_guid(context.Request.Params["PostID"]), ref responseText);
                    _return_response(ref responseText);
                    return;
                case "LikeDislikePost":
                case "LikeDislikeComment":
                    {
                        Guid? objectId = PublicMethods.parse_guid(context.Request.Params[
                            command == "LikeDislikePost" ? "PostID" : "CommentID"]);

                        double? score = PublicMethods.parse_double(context.Request.Params["Score"]);

                        if (command == "LikeDislikePost")
                            like_dislike_post(objectId,
                                PublicMethods.parse_bool(context.Request.Params["Like"], true), score, DateTime.Now, ref responseText);
                        else
                            like_dislike_comment(objectId,
                                PublicMethods.parse_bool(context.Request.Params["Like"], true), score, DateTime.Now, ref responseText);
                        _return_response(ref responseText);
                    }
                    return;
                case "UnlikePost":
                case "UnlikeComment":
                    if (command == "UnlikePost")
                        unlike(PublicMethods.parse_guid(context.Request.Params["PostID"]), true, ref responseText);
                    else
                        unlike(PublicMethods.parse_guid(context.Request.Params["CommentID"]), false, ref responseText);

                    _return_response(ref responseText);
                    return;
                case "GetFans":
                    {
                        bool? likeStatus = PublicMethods.parse_bool(context.Request.Params["LikeStatus"]);
                        int? cnt = PublicMethods.parse_int(context.Request.Params["Count"]);
                        long? lowerBoundary = PublicMethods.parse_long(context.Request.Params["LowerBoundary"]);

                        Guid? postId = PublicMethods.parse_guid(context.Request.Params["PostID"]);
                        Guid? commentId = PublicMethods.parse_guid(context.Request.Params["CommentID"]);

                        if (postId.HasValue && postId != Guid.Empty)
                            get_fans(postId.Value, true, likeStatus, cnt, lowerBoundary, ref responseText);
                        else if (commentId.HasValue && commentId != Guid.Empty)
                            get_fans(commentId.Value, false, likeStatus, cnt, lowerBoundary, ref responseText);
                        _return_response(ref responseText);
                    }
                    return;
                case "GetPostsCount":
                    get_posts_count(PublicMethods.parse_guid(context.Request.Params["OwnerID"]),
                        PublicMethods.parse_guid("SenderUserID"), ref responseText);
                    _return_response(ref responseText);
                    return;
                case "GetSharesCount":
                    get_shares_count(PublicMethods.parse_guid(context.Request.Params["PostID"]), ref responseText);
                    _return_response(ref responseText);
                    return;
                case "GetCommentsCount":
                    get_comments_count(PublicMethods.parse_guid(context.Request.Params["PostID"]),
                        PublicMethods.parse_guid("SenderUserID"), ref responseText);
                    _return_response(ref responseText);
                    return;
                case "GetUserPostsCount":
                    get_user_posts_count(PublicMethods.parse_guid(context.Request.Params["UserID"]),
                        SharingUtilities.get_post_type_id(PublicMethods.parse_string(context.Request.Params["PostType"], false)),
                        ref responseText);
                    _return_response(ref responseText);
                    return;
                case "GetPostLikesDislikesCount":
                case "GetCommentLikesDislikesCount":
                    if (command == "GetPostLikesDislikesCount")
                        get_post_likes_dislikes_count(PublicMethods.parse_guid(context.Request.Params["PostID"]),
                            PublicMethods.parse_bool(context.Request.Params["Like"], true), ref responseText);
                    else
                        get_comment_likes_dislikes_count(PublicMethods.parse_guid(context.Request.Params["CommentID"]),
                            PublicMethods.parse_bool(context.Request.Params["Like"], true), ref responseText);

                    _return_response(ref responseText);
                    return;
            }

            paramsContainer.return_response(PublicConsts.BadRequestResponse);
        }

        protected void _return_response(ref string responseText)
        {
            paramsContainer.return_response(ref responseText);
        }

        protected void add_post(int postTypeId, string description, Guid? sharedObjectId, DateTime? sendDate,
            Guid? ownerId, string ownerType, PrivacyTypes privacy, DocFileInfo attachedFile, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!string.IsNullOrEmpty(description) && description.Length > 3900)
            {
                responseText = "{\"ErrorText\":\"" + Messages.MaxAllowedInputLengthExceeded + "\"}";
                return;
            }
            else if (!PublicMethods.is_secure_title(description))
            {
                responseText = "{\"ErrorText\":\"" + Messages.TheTextIsFormattedBadly + "\"}";
                return;
            }

            string strPrivacy = privacy.ToString();

            Post postInfo = new Post()
            {
                PostID = Guid.NewGuid(),
                PostTypeID = postTypeId,
                OriginalDescription = description,
                SharedObjectID = sharedObjectId,
                OriginalSendDate = sendDate,
                OwnerID = ownerId,
                OwnerType = ownerType,
                Privacy = strPrivacy
            };

            postInfo.OriginalSender.UserID = paramsContainer.CurrentUserID;

            bool fileMoved = false;

            if (attachedFile != null && attachedFile.FileID.HasValue)
            {
                attachedFile.Extension = "jpg";

                postInfo.HasPicture = true;

                attachedFile.move(paramsContainer.Tenant.Id, FolderNames.TemporaryFiles, FolderNames.Pictures, postInfo.PostID);

                fileMoved = true;
            }

            bool result = SharingController.add_post(paramsContainer.Tenant.Id, postInfo);

            if (!result)
            {
                if (fileMoved)
                {
                    Guid? oldId = attachedFile.FileID;
                    attachedFile.FileID = postInfo.PostID;

                    attachedFile.move(paramsContainer.Tenant.Id, FolderNames.Pictures, FolderNames.TemporaryFiles, oldId);
                }

                responseText = "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";
                return;
            }

            //responseText = _create_post_json(postInfo, currentUserId);
            Post addedPost = SharingController.get_post(paramsContainer.Tenant.Id,
                postInfo.PostID.Value, paramsContainer.CurrentUserID.Value);

            responseText = (addedPost == null ? postInfo : addedPost)
                .toJson(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID, false);

            //Save Tagged Items
            if (result)
            {
                List<TaggedItem> tagged = new List<TaggedItem>();

                List<InlineTag> tags = Expressions.get_tagged_items(description);

                foreach (InlineTag tg in tags)
                {
                    TaggedType tgTp = TaggedType.None;
                    if (!Enum.TryParse(tg.Type, out tgTp) || tgTp == TaggedType.None || !tg.ID.HasValue) continue;

                    tagged.Add(new TaggedItem(postInfo.PostID.Value, tg.ID.Value, TagContextType.Post, tgTp));
                }

                GlobalController.save_tagged_items_offline(paramsContainer.Tenant.Id,
                    tagged, false, paramsContainer.CurrentUserID.Value);
            }
            //Save Tagged Items

            //Send Notification
            if (result && ownerId.HasValue)
            {
                Notification not = new Notification()
                {
                    SubjectID = postInfo.PostID,
                    RefItemID = ownerId,
                    SubjectType = SubjectType.Post,
                    Action = ActionType.Post,
                    Description = description
                };

                if (ownerType == PostOwnerType.User.ToString()) not.UserID = ownerId;
                else if (ownerType == PostOwnerType.WFHistory.ToString())
                {
                    History hist = WFController.get_history(paramsContainer.Tenant.Id, ownerId.Value);

                    if (hist != null && hist.DirectorNode.NodeID.HasValue)
                    {
                        not.Audience[UserStatus.Director] = CNController.get_members(paramsContainer.Tenant.Id,
                            hist.DirectorNode.NodeID.Value, pending: false, admin: null)
                            .Where(x => x.Member.UserID != paramsContainer.CurrentUserID.Value)
                            .Select(u => u.Member.UserID.Value).ToList();
                    }
                }

                not.Sender.UserID = paramsContainer.CurrentUserID;
                NotificationController.send_notification(paramsContainer.Tenant.Id, not);
            }
            //end of Send Notification

            //Save Log
            if (result)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = postInfo.SendDate,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.SendPost,
                    SubjectID = postInfo.PostID,
                    Info = "{\"Description\":\"" + Base64.encode(description) + "\"}",
                    ModuleIdentifier = ModuleIdentifier.SH
                });
            }
            //end of Save Log

            //Send RealTime Data
            if (result && RaaiVanSettings.RealTime(paramsContainer.Tenant.Id) && ownerId.HasValue && ownerId != Guid.Empty)
            {
                string feedIds = ownerId.Value.ToString() + "," + postInfo.PostID.Value.ToString();
                RaaiVanHub.SendData(paramsContainer.Tenant.Id, feedIds.Split(',').ToList(), RaaiVanHub.RealTimeAction.NewPost, responseText);
            }
            //end of Send RealTime Data
        }

        protected void update_post(Guid? postId, string description, DateTime? lastModificationDate, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!string.IsNullOrEmpty(description) && description.Length > 3900)
            {
                responseText = "{\"ErrorText\":\"" + Messages.MaxAllowedInputLengthExceeded + "\"}";
                return;
            }

            if (!postId.HasValue ||
                (!PublicMethods.is_system_admin(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID.Value) &&
                SharingController.get_post_sender_id(paramsContainer.Tenant.Id, postId.Value) != paramsContainer.CurrentUserID))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                return;
            }

            Post postInfo = new Post()
            {
                PostID = postId,
                Description = description,
                LastModifierUserID = paramsContainer.CurrentUserID,
                LastModificationDate = lastModificationDate
            };

            bool result = SharingController.update_post(paramsContainer.Tenant.Id, postInfo);

            if (!result)
            {
                responseText = "{\"ErrorText\":\"" + Messages.OperationFailed.ToString() + "\"}";
                return;
            }

            responseText = "{\"PostID\":\"" + postId.ToString() + "\"" + 
                ",\"Description\":\"" + Base64.encode(description) + "\"" + 
                ",\"LastModifierUserID\":\"" + paramsContainer.CurrentUserID.Value.ToString() + "\"" + 
                ",\"LastModificationDate\":\"" + GenericDate.get_local_date(lastModificationDate) + "\"" + 
                ",\"GregorianLastModificationDate\":\"" + lastModificationDate + "\"" + 
                "}";

            //Save Tagged Items
            if (result)
            {
                List<TaggedItem> tagged = new List<TaggedItem>();

                List<InlineTag> tags = Expressions.get_tagged_items(description);

                foreach (InlineTag tg in tags)
                {
                    TaggedType tgTp = TaggedType.None;
                    if (!Enum.TryParse(tg.Type, out tgTp) || tgTp == TaggedType.None || !tg.ID.HasValue) continue;

                    tagged.Add(new TaggedItem(postInfo.PostID.Value, tg.ID.Value, TagContextType.Post, tgTp));
                }

                GlobalController.save_tagged_items_offline(paramsContainer.Tenant.Id,
                    tagged, true, paramsContainer.CurrentUserID.Value);
            }
            //Save Tagged Items

            //Send Notification
            if (result)
            {
                Post oldPost = SharingController.get_post(paramsContainer.Tenant.Id, postId.Value, null);

                if (oldPost != null && oldPost.OwnerID.HasValue && oldPost.OwnerType != PostOwnerType.WFHistory.ToString()) {
                    Notification not = new Notification()
                    {
                        SubjectID = postInfo.PostID,
                        RefItemID = oldPost.OwnerID,
                        SubjectType = SubjectType.Post,
                        Action = ActionType.Post,
                        Description = description
                    };

                    if (oldPost.OwnerType == PostOwnerType.User.ToString()) not.UserID = oldPost.OwnerID;

                    not.Sender.UserID = paramsContainer.CurrentUserID;
                    NotificationController.send_notification(paramsContainer.Tenant.Id, not);
                }
            }
            //end of Send Notification

            //Save Log
            if (result)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = postInfo.LastModificationDate,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.ModifyPost,
                    SubjectID = postInfo.PostID,
                    Info = "{\"Description\":\"" + Base64.encode(description) + "\"}",
                    ModuleIdentifier = ModuleIdentifier.SH
                });
            }
            //end of Save Log

            //Send RealTime Data
            if (result && RaaiVanSettings.RealTime(paramsContainer.Tenant.Id) && postId.HasValue && postId != Guid.Empty)
            {
                Guid? ownerId = SharingController.get_post_owner_id(paramsContainer.Tenant.Id, postId.Value);
                if (ownerId.HasValue)
                {
                    string feedIds = ownerId.ToString() + "," + postId.Value.ToString();
                    RaaiVanHub.SendData(paramsContainer.Tenant.Id, feedIds.Split(',').ToList(), RaaiVanHub.RealTimeAction.ModifyPost, responseText);
                }
            }
            //end of Send RealTime Data
        }

        protected bool is_admin(Guid postIdOrCommentId)
        {
            if (!paramsContainer.CurrentUserID.HasValue) return false;

            Guid? ownerId = SharingController.get_post_owner_id(paramsContainer.Tenant.Id, postIdOrCommentId);

            if (!ownerId.HasValue || !CNController.is_node(paramsContainer.Tenant.Id, ownerId.Value)) return false;

            return
                PublicMethods.is_system_admin(paramsContainer.Tenant.Id,
                    paramsContainer.CurrentUserID.Value) ||
                CNController.is_service_admin(paramsContainer.Tenant.Id,
                    ownerId.Value, paramsContainer.CurrentUserID.Value) ||
                CNController.is_node_admin(paramsContainer.Tenant.Id,
                    paramsContainer.CurrentUserID.Value, ownerId.Value, null, null, null) ||
                (
                    CNController.has_extension(paramsContainer.Tenant.Id, ownerId.Value, ExtensionType.Group) &&
                    CNController.is_admin_member(paramsContainer.Tenant.Id, ownerId.Value, paramsContainer.CurrentUserID.Value)
                );
        }

        protected void remove_post(Guid? postId, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            bool isAdmin = postId.HasValue && is_admin(postId.Value);

            if (!postId.HasValue ||
                (!isAdmin && !PublicMethods.is_system_admin(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID.Value) &&
                SharingController.get_post_sender_id(paramsContainer.Tenant.Id, postId.Value) != paramsContainer.CurrentUserID))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                return;
            }

            bool result = SharingController.remove_post(paramsContainer.Tenant.Id, postId);

            responseText = result ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully.ToString() + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed.ToString() + "\"}";

            //Remove Notifications
            if (result)
            {
                NotificationController.remove_notifications(paramsContainer.Tenant.Id,
                    new Notification() { RefItemID = postId });
                NotificationController.remove_notifications(paramsContainer.Tenant.Id,
                    new Notification() { SubjectID = postId });
            }
            //end of Remove Notifications

            //Save Log
            if (result)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = DateTime.Now,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.RemovePost,
                    SubjectID = postId,
                    ModuleIdentifier = ModuleIdentifier.SH
                });
            }
            //end of Save Log

            //Send RealTime Data
            if (result && RaaiVanSettings.RealTime(paramsContainer.Tenant.Id) && postId.HasValue && postId != Guid.Empty)
            {
                Guid? ownerId = SharingController.get_post_owner_id(paramsContainer.Tenant.Id, postId.Value);
                if (ownerId.HasValue)
                {
                    string feedIds = ownerId.ToString() + "," + postId.Value.ToString();
                    RaaiVanHub.SendData(paramsContainer.Tenant.Id, feedIds.Split(',').ToList(), RaaiVanHub.RealTimeAction.RemovePost,
                        "{\"PostID\":\"" + postId.ToString() + "\"}");
                }
            }
            //end of Send RealTime Data
        }

        protected void get_post(Guid? postId, bool? ownerInfo, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBView) return;

            Guid? ownerId = !postId.HasValue ? null :
                SharingController.get_post_owner_id(paramsContainer.Tenant.Id, postId.Value);

            bool isNode = ownerId.HasValue && postId.HasValue && CNController.is_node(paramsContainer.Tenant.Id, ownerId.Value);
            bool isSystemAdmin = postId.HasValue &&
                PublicMethods.is_system_admin(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID.Value);
            bool perServiceAdmin = postId.HasValue && isNode && (isSystemAdmin || 
                CNController.is_service_admin(paramsContainer.Tenant.Id, ownerId.Value, paramsContainer.CurrentUserID.Value));
            bool perNodeAdmin = postId.HasValue && isNode && (perServiceAdmin || 
                CNController.is_node_admin(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID.Value, ownerId.Value, null, null, null));

            if (!postId.HasValue || (isNode && !perNodeAdmin &&
                !PrivacyController.check_access(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID,
                    ownerId.Value, PrivacyObjectType.None, PermissionType.View)))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                return;
            }

            Post post = !postId.HasValue ? null :
                SharingController.get_post(paramsContainer.Tenant.Id, postId.Value, paramsContainer.CurrentUserID);

            if (post == null)
            {
                responseText = "{}";
                return;
            }

            post.Comments = !postId.HasValue ? new List<Comment>() :
                SharingController.get_post_comments(paramsContainer.Tenant.Id, postId.Value, paramsContainer.CurrentUserID);

            string strOwnerInfo = "{}";

            if (post.OwnerID.HasValue)
            {
                PostOwnerType ownerType = new PostOwnerType();
                try { ownerType = (PostOwnerType)Enum.Parse(typeof(PostOwnerType), post.OwnerType); }
                catch { ownerType = PostOwnerType.None; }

                switch (ownerType)
                {
                    case PostOwnerType.User:
                        User ownerUser = UsersController.get_user(paramsContainer.Tenant.Id, post.OwnerID.Value);
                        if (ownerUser == null) break;
                        post.OwnerTitle = (ownerUser.FirstName + " " + ownerUser.LastName).Trim();
                        strOwnerInfo = "{\"ID\":\"" + ownerUser.UserID.Value.ToString() + "\"" + 
                            ",\"Title\":\"" + Base64.encode(post.OwnerTitle) + "\"" + 
                            ",\"ImageURL\":\"" + DocumentUtilities.get_personal_image_address(
                                paramsContainer.Tenant.Id, ownerUser.UserID.Value) + "\"" + 
                            ",\"NavigateURL\":\"" + PublicConsts.get_client_url(PublicConsts.ProfilePage) +
                                "/" + ownerUser.UserID.Value.ToString() + "\"}";
                        break;
                    case PostOwnerType.Node:
                    case PostOwnerType.Knowledge:
                        if (!ownerInfo.HasValue || !ownerInfo.Value) break;
                        Node node = CNController.get_node(paramsContainer.Tenant.Id, post.OwnerID.Value);
                        if (node == null) break;
                        strOwnerInfo = "{\"ID\":\"" + node.NodeID.Value.ToString() + "\"" + 
                            ",\"Title\":\"" + Base64.encode(node.Name) + "\"" + 
                            ",\"Type\":\"" + Base64.encode(node.NodeType) + "\"" + 
                            ",\"ImageURL\":\"" +
                                DocumentUtilities.get_icon_url(paramsContainer.Tenant.Id, node.NodeID.Value) + "\"" + 
                            ",\"NavigateURL\":\"" + PublicConsts.get_client_url(PublicConsts.NodePage) +
                                "/" + node.NodeID.Value.ToString() + "\"}";
                        break;
                    case PostOwnerType.WFHistory:
                        if (!ownerInfo.HasValue || !ownerInfo.Value) break;
                        History hist = WFController.get_history(paramsContainer.Tenant.Id, post.OwnerID.Value);
                        if (hist == null || !hist.OwnerID.HasValue) break;
                        Node nd = CNController.get_node(paramsContainer.Tenant.Id, hist.OwnerID.Value);
                        if (nd == null) break;
                        strOwnerInfo = "{\"ID\":\"" + nd.NodeID.Value.ToString() + "\"" +
                            ",\"Title\":\"" + Base64.encode(nd.Name) + "\"" +
                            ",\"Type\":\"" + Base64.encode(nd.NodeType) + "\"" +
                            ",\"IsWorkFlow\":" + true.ToString().ToLower() +
                            ",\"ImageURL\":\"" +
                                DocumentUtilities.get_icon_url(paramsContainer.Tenant.Id, nd.NodeID.Value) + "\"" +
                            ",\"NavigateURL\":\"" + PublicConsts.get_client_url(PublicConsts.NodePage) +
                                "/" + nd.NodeID.Value.ToString() + "\"}";
                        break;
                }
            }

            responseText = "{\"OwnerInfo\":" + strOwnerInfo + 
                ",\"Post\":" + post.toJson(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID, perNodeAdmin) + "}";
        }

        protected void get_posts(Guid? ownerId, bool? ownerInfo, bool? news,
            DateTime? maxDate, DateTime? minDate, int? count, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBView) return;

            bool accessDenied = false;
            if (!ownerId.HasValue) accessDenied = true;

            bool isNode = !accessDenied && CNController.is_node(paramsContainer.Tenant.Id, ownerId.Value);
            bool isSystemAdmin = !accessDenied && paramsContainer.CurrentUserID.HasValue &&
                PublicMethods.is_system_admin(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID.Value);
            bool perServiceAdmin = paramsContainer.CurrentUserID.HasValue &&
                (isSystemAdmin || (isNode && CNController.is_service_admin(
                paramsContainer.Tenant.Id, ownerId.Value, paramsContainer.CurrentUserID.Value)));
            bool perNodeAdmin = paramsContainer.CurrentUserID.HasValue &&
                (perServiceAdmin || (isNode && CNController.is_node_admin(paramsContainer.Tenant.Id,
                paramsContainer.CurrentUserID.Value, ownerId.Value, null, null, null)));

            if (!accessDenied && isNode && !perNodeAdmin &&
                !CNController.is_node_member(paramsContainer.Tenant.Id, ownerId.Value, paramsContainer.CurrentUserID.Value) &&
                !PrivacyController.check_access(paramsContainer.Tenant.Id,
                    paramsContainer.CurrentUserID, ownerId.Value, PrivacyObjectType.Node, PermissionType.View)) accessDenied = true;

            if (accessDenied)
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                return;
            }

            bool isAdmin = isSystemAdmin || (isNode && (perNodeAdmin || (
                CNController.has_extension(paramsContainer.Tenant.Id, ownerId.Value, ExtensionType.Group) &&
                CNController.is_admin_member(paramsContainer.Tenant.Id, ownerId.Value, paramsContainer.CurrentUserID.Value))));

            List<Post> posts = SharingController.get_posts(paramsContainer.Tenant.Id,
                ownerId, paramsContainer.CurrentUserID, news, maxDate, minDate, !count.HasValue ? 20 : count.Value);

            List<Comment> comments = SharingController.get_post_comments(paramsContainer.Tenant.Id,
                posts.Select(u => u.PostID.Value).ToList(), paramsContainer.CurrentUserID);

            List<Guid> ownerUserIds = posts.Where(u => u.OwnerType == PostOwnerType.User.ToString() && u.OwnerID.HasValue)
                .Select(v => v.OwnerID.Value).ToList();

            List<User> ownerUsers = new List<User>();

            if (ownerUserIds != null && ownerUserIds.Count > 0)
                ownerUsers = UsersController.get_users(paramsContainer.Tenant.Id, ownerUserIds);

            if (ownerUsers != null && ownerUsers.Count > 0)
                for (int i = 0; i < posts.Count; ++i)
                {
                    User _user = ownerUsers.Where(u => u.UserID == posts[i].OwnerID).FirstOrDefault();
                    if (_user == null) continue;
                    posts[i].OwnerTitle = (_user.FirstName + " " + _user.LastName).Trim();
                }

            var firstDate = DateTime.MinValue;
            try { firstDate = posts.First().SendDate.Value; }
            catch { firstDate = DateTime.MinValue; }

            var lastDate = DateTime.MaxValue;
            try { lastDate = posts.Last().SendDate.Value; }
            catch { lastDate = DateTime.MaxValue; }

            posts.ForEach(p => p.Comments = comments.Where(u => u.PostID == p.PostID).ToList());

            responseText = "{\"Posts\":[" + string.Join(",",
                    posts.Select(p => p.toJson(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID, isAdmin))) + "]" +
                ",\"FirstDate\":\"" + firstDate.ToString() + "\",\"LastDate\":\"" + lastDate.ToString() + "\"}";
        }

        protected void share(Guid? refPostId, Guid? ownerId, string description, DateTime? sendDate,
            PrivacyTypes privacy, string ownerType, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!string.IsNullOrEmpty(description) && description.Length > 3900)
            {
                responseText = "{\"ErrorText\":\"" + Messages.MaxAllowedInputLengthExceeded + "\"}";
                return;
            }
            else if (!PublicMethods.is_secure_title(description))
            {
                responseText = "{\"ErrorText\":\"" + Messages.TheTextIsFormattedBadly + "\"}";
                return;
            }

            Guid? refOwnerId = !refPostId.HasValue ? null :
                SharingController.get_post_owner_id(paramsContainer.Tenant.Id, refPostId.Value);

            bool accessDenied = refOwnerId == Guid.Empty;

            bool isNode = !accessDenied && refOwnerId.HasValue && CNController.is_node(paramsContainer.Tenant.Id, refOwnerId.Value);
            bool isSystemAdmin = !accessDenied &&
                PublicMethods.is_system_admin(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID.Value);
            bool perServiceAdmin = isSystemAdmin || (isNode && CNController.is_service_admin(
                paramsContainer.Tenant.Id, refOwnerId.Value, paramsContainer.CurrentUserID.Value));
            bool perNodeAdmin = perServiceAdmin || (isNode && CNController.is_node_admin(
                paramsContainer.Tenant.Id, paramsContainer.CurrentUserID.Value, refOwnerId.Value, null, null, null));

            if (accessDenied || (isNode && !perNodeAdmin &&
                !PrivacyController.check_access(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID,
                    refOwnerId.Value, PrivacyObjectType.None, PermissionType.View)))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                return;
            }

            Post postInfo = new Post()
            {
                PostID = Guid.NewGuid(),
                RefPostID = refPostId,
                OwnerID = ownerId,
                Description = description,
                SendDate = sendDate,
                Privacy = privacy.ToString(),
                OwnerType = ownerType
            };

            postInfo.Sender.UserID = paramsContainer.CurrentUserID;

            bool result = SharingController.share(paramsContainer.Tenant.Id, postInfo);

            Post shared = result ? SharingController.get_post(paramsContainer.Tenant.Id,
                postInfo.PostID.Value, paramsContainer.CurrentUserID) : null;

            responseText = !result ? "{\"ErrorText\":\"" + Messages.OperationFailed.ToString() + "\"}" :
                "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully.ToString() + "\"" +
                ",\"Post\":" + (shared == null ? postInfo : shared)
                    .toJson(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID, perNodeAdmin) + 
                "}";

            //Send Notification
            if (result && ownerId.HasValue)
            {
                Notification not = new Notification()
                {
                    SubjectID = postInfo.PostID,
                    RefItemID = ownerId,
                    SubjectType = SubjectType.Post,
                    Action = Modules.NotificationCenter.ActionType.Share,
                    Description = description
                };
                if (ownerType == PostOwnerType.User.ToString()) not.UserID = ownerId;
                not.Sender.UserID = paramsContainer.CurrentUserID;
                NotificationController.send_notification(paramsContainer.Tenant.Id, not);
            }
            //end of Send Notification

            //Save Log
            if (result)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = postInfo.Sender.UserID,
                    Date = DateTime.Now,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.SharePost,
                    SubjectID = postInfo.PostID,
                    SecondSubjectID = postInfo.RefPostID,
                    Info = "{\"Description\":\"" + Base64.encode(description) + "\"}",
                    ModuleIdentifier = ModuleIdentifier.SH
                });
            }
            //end of Save Log

            //Send RealTime Data
            if (result && RaaiVanSettings.RealTime(paramsContainer.Tenant.Id) && ownerId.HasValue && ownerId != Guid.Empty)
            {
                string feedIds = ownerId.Value.ToString() + "," + postInfo.PostID.Value.ToString();
                RaaiVanHub.SendData(paramsContainer.Tenant.Id, feedIds.Split(',').ToList(), RaaiVanHub.RealTimeAction.NewPost, responseText);
            }
            //end of Send RealTime Data
        }

        protected void add_comment(Guid? postId, string description, DateTime? sendDate, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!string.IsNullOrEmpty(description) && description.Length > 3900)
            {
                responseText = "{\"ErrorText\":\"" + Messages.MaxAllowedInputLengthExceeded + "\"}";
                return;
            }

            Guid? ownerId = !postId.HasValue ? null :
                SharingController.get_post_owner_id(paramsContainer.Tenant.Id, postId.Value);

            if (!ownerId.HasValue || (CNController.is_node(paramsContainer.Tenant.Id, ownerId.Value) &&
                !PublicMethods.is_system_admin(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID.Value) &&
                !PrivacyController.check_access(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID,
                    ownerId.Value, PrivacyObjectType.None, PermissionType.View) &&
                !CNController.is_service_admin(paramsContainer.Tenant.Id, ownerId.Value, paramsContainer.CurrentUserID.Value) &&
                !CNController.is_node_admin(paramsContainer.Tenant.Id,
                    paramsContainer.CurrentUserID.Value, ownerId.Value, null, null, null)))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                return;
            }

            Comment commentInfo = new Comment()
            {
                CommentID = Guid.NewGuid(),
                PostID = postId,
                Description = description,
                SendDate = sendDate
            };

            commentInfo.Sender.UserID = paramsContainer.CurrentUserID;

            bool result = SharingController.add_comment(paramsContainer.Tenant.Id, commentInfo);

            if (!result)
            {
                responseText = "{\"ErrorText\":\"" + Messages.OperationFailed.ToString() + "\"}";
                return;
            }

            Comment addedComment = SharingController.get_comment(paramsContainer.Tenant.Id,
                commentInfo.CommentID.Value, paramsContainer.CurrentUserID.Value);

            responseText = (addedComment == null ? commentInfo : addedComment)
                .toJson(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID, false);

            //Save Tagged Items
            if (result)
            {
                List<TaggedItem> tagged = new List<TaggedItem>();

                List<InlineTag> tags = Expressions.get_tagged_items(description);

                foreach (InlineTag tg in tags)
                {
                    TaggedType tgTp = TaggedType.None;
                    if (!Enum.TryParse(tg.Type, out tgTp) || tgTp == TaggedType.None || !tg.ID.HasValue) continue;

                    tagged.Add(new TaggedItem(commentInfo.CommentID.Value, tg.ID.Value, TagContextType.Comment, tgTp));
                }

                GlobalController.save_tagged_items_offline(paramsContainer.Tenant.Id,
                    tagged, false, paramsContainer.CurrentUserID.Value);
            }
            //Save Tagged Items

            //Send Notification
            if (result)
            {
                Notification not = new Notification()
                {
                    SubjectID = commentInfo.CommentID,
                    RefItemID = postId,
                    SubjectType = SubjectType.Comment,
                    Action = ActionType.Comment,
                    Description = description
                };
                not.Sender.UserID = paramsContainer.CurrentUserID;
                NotificationController.send_notification(paramsContainer.Tenant.Id, not);
            }
            //end of Send Notification

            //Save Log
            if (result)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = commentInfo.Sender.UserID,
                    Date = commentInfo.SendDate,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.SendComment,
                    SubjectID = commentInfo.CommentID,
                    Info = "{\"Description\":\"" + Base64.encode(description) + "\"}",
                    ModuleIdentifier = ModuleIdentifier.SH
                });
            }
            //end of Save Log

            //Send RealTime Data
            if (result && RaaiVanSettings.RealTime(paramsContainer.Tenant.Id) && ownerId != Guid.Empty)
            {
                string feedIds = ownerId.ToString() + "," + postId.Value.ToString();
                RaaiVanHub.SendData(paramsContainer.Tenant.Id, feedIds.Split(',').ToList(), RaaiVanHub.RealTimeAction.NewComment, responseText);
            }
            //end of Send RealTime Data
        }

        protected void update_comment(Guid? commentId, string description, DateTime? lastModificationDate, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!string.IsNullOrEmpty(description) && description.Length > 3900)
            {
                responseText = "{\"ErrorText\":\"" + Messages.MaxAllowedInputLengthExceeded + "\"}";
                return;
            }

            if (!commentId.HasValue ||
                (!PublicMethods.is_system_admin(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID.Value) &&
                SharingController.get_comment_sender_id(paramsContainer.Tenant.Id, commentId.Value) != paramsContainer.CurrentUserID))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                return;
            }

            Comment commentInfo = new Comment()
            {
                CommentID = commentId,
                Description = description,
                LastModifierUserID = paramsContainer.CurrentUserID,
                LastModificationDate = lastModificationDate
            };

            bool result = SharingController.update_comment(paramsContainer.Tenant.Id, commentInfo);

            if (!result)
            {
                responseText = "{\"ErrorText\":\"" + Messages.OperationFailed.ToString() + "\"}";
                return;
            }

            responseText = "{\"CommentID\":\"" + commentId.ToString() + "\"" + 
                ",\"Description\":\"" + Base64.encode(description) + "\"" + 
                ",\"LastModifierUserID\":\"" + paramsContainer.CurrentUserID.ToString() + "\"" + 
                ",\"LastModificationDate\":\"" + GenericDate.get_local_date(lastModificationDate) + "\"" + 
                ",\"GregorianLastModificationDate\":\"" + lastModificationDate + "\"" + 
                "}";

            //Save Tagged Items
            if (result)
            {
                List<TaggedItem> tagged = new List<TaggedItem>();

                List<InlineTag> tags = Expressions.get_tagged_items(description);

                foreach (InlineTag tg in tags)
                {
                    TaggedType tgTp = TaggedType.None;
                    if (!Enum.TryParse(tg.Type, out tgTp) || tgTp == TaggedType.None || !tg.ID.HasValue) continue;

                    tagged.Add(new TaggedItem(commentInfo.CommentID.Value, tg.ID.Value, TagContextType.Comment, tgTp));
                }

                GlobalController.save_tagged_items_offline(paramsContainer.Tenant.Id,
                    tagged, true, paramsContainer.CurrentUserID.Value);
            }
            //Save Tagged Items

            //Save Log
            if (result)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = commentInfo.LastModifierUserID,
                    Date = commentInfo.LastModificationDate,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.ModifyComment,
                    SubjectID = commentInfo.CommentID,
                    Info = "{\"Description\":\"" + Base64.encode(description) + "\"}",
                    ModuleIdentifier = ModuleIdentifier.SH
                });
            }
            //end of Save Log

            //Send RealTime Data
            if (result && RaaiVanSettings.RealTime(paramsContainer.Tenant.Id) && commentId.HasValue && commentId != Guid.Empty)
            {
                Guid? ownerId = SharingController.get_post_owner_id(paramsContainer.Tenant.Id, commentId.Value);
                Comment comment = SharingController.get_comment(paramsContainer.Tenant.Id, commentId.Value, null);

                if (ownerId.HasValue && comment != null)
                {
                    string feedIds = ownerId.ToString() + "," + comment.PostID.Value.ToString();
                    RaaiVanHub.SendData(paramsContainer.Tenant.Id, feedIds.Split(',').ToList(), RaaiVanHub.RealTimeAction.ModifyComment, responseText);
                }
            }
            //end of Send RealTime Data
        }

        protected void remove_comment(Guid? commentId, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            bool isAdmin = commentId.HasValue && is_admin(commentId.Value);

            if (!commentId.HasValue ||
                (!isAdmin && !PublicMethods.is_system_admin(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID.Value) &&
                SharingController.get_comment_sender_id(paramsContainer.Tenant.Id, commentId.Value) != paramsContainer.CurrentUserID &&
                SharingController.get_post_sender_id(paramsContainer.Tenant.Id, commentId.Value) != paramsContainer.CurrentUserID &&
                SharingController.get_post_owner_id(paramsContainer.Tenant.Id, commentId.Value) != paramsContainer.CurrentUserID))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                return;
            }

            bool result = SharingController.remove_comment(paramsContainer.Tenant.Id, commentId);

            responseText = result ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully.ToString() + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed.ToString() + "\"}";

            //Remove Notifications
            if (result) NotificationController.remove_notifications(paramsContainer.Tenant.Id,
                new Notification() { SubjectID = commentId });
            //end of Remove Notifications

            //Save Log
            if (result)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = DateTime.Now,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.RemoveComment,
                    SubjectID = commentId,
                    ModuleIdentifier = ModuleIdentifier.SH
                });
            }
            //end of Save Log

            //Send RealTime Data
            if (result && RaaiVanSettings.RealTime(paramsContainer.Tenant.Id) && commentId.HasValue && commentId != Guid.Empty)
            {
                Guid? ownerId = SharingController.get_post_owner_id(paramsContainer.Tenant.Id, commentId.Value);
                Comment comment = SharingController.get_comment(paramsContainer.Tenant.Id, commentId.Value, null);

                if (ownerId.HasValue && comment != null)
                {
                    string feedIds = ownerId.ToString() + "," + comment.PostID.Value.ToString();
                    RaaiVanHub.SendData(paramsContainer.Tenant.Id, feedIds.Split(',').ToList(), RaaiVanHub.RealTimeAction.RemoveComment,
                        "{\"CommentID\":\"" + commentId.ToString() + "\"}");
                }
            }
            //end of Send RealTime Data
        }

        protected void get_comments(Guid? postId, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBView) return;

            Guid? ownerId = !postId.HasValue ? null :
                SharingController.get_post_owner_id(paramsContainer.Tenant.Id, postId.Value);

            bool accessDenied = ownerId == Guid.Empty;

            bool isNode = !accessDenied && ownerId.HasValue && CNController.is_node(paramsContainer.Tenant.Id, ownerId.Value);
            bool isSystemAdmin = !accessDenied &&
                PublicMethods.is_system_admin(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID.Value);
            bool perServiceAdmin = isNode && (isSystemAdmin || CNController.is_service_admin(
                paramsContainer.Tenant.Id, ownerId.Value, paramsContainer.CurrentUserID.Value));
            bool perNodeAdmin = isNode && (perServiceAdmin || CNController.is_node_admin(paramsContainer.Tenant.Id,
                paramsContainer.CurrentUserID.Value, ownerId.Value, null, null, null));

            if (accessDenied || (isNode && !perNodeAdmin &&
                !PrivacyController.check_access(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID,
                    ownerId.Value, PrivacyObjectType.Node, PermissionType.View)))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                return;
            }

            List<Comment> comments = SharingController.get_post_comments(paramsContainer.Tenant.Id,
                postId.Value, paramsContainer.CurrentUserID);

            responseText = "{\"Comments\":[" + string.Join(",",
                comments.Select(u => u.toJson(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID, perNodeAdmin))) + "]}";
        }

        protected void like_dislike_post(Guid? postId, bool? like, double? score, DateTime date, ref string responseText)
        {
            if (!paramsContainer.GBEdit) return;

            LikeDislike info = new LikeDislike()
            {
                ObjectID = postId,
                UserID = paramsContainer.CurrentUserID,
                Like = like.HasValue && like.Value,
                Score = score,
                Date = date
            };

            bool result = postId.HasValue && SharingController.like_dislike_post(paramsContainer.Tenant.Id, info);

            responseText = result ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully.ToString() + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed.ToString() + "\"}";

            //Send Notification
            if (result)
            {
                Notification not = new Notification()
                {
                    SubjectID = postId,
                    RefItemID = postId,
                    SubjectType = SubjectType.Post,
                    Action = like.HasValue && like.Value ?
                        Modules.NotificationCenter.ActionType.Like : Modules.NotificationCenter.ActionType.Dislike
                };
                not.Sender.UserID = paramsContainer.CurrentUserID;
                NotificationController.send_notification(paramsContainer.Tenant.Id, not);

                not = new Notification()
                {
                    SubjectID = postId,
                    Action = like.HasValue && like.Value ?
                        Modules.NotificationCenter.ActionType.Dislike : Modules.NotificationCenter.ActionType.Like
                };
                not.Sender.UserID = paramsContainer.CurrentUserID;
                NotificationController.remove_notifications(paramsContainer.Tenant.Id, not);
            }
            //end of Send Notification

            //Save Log
            if (result)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = date,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = like.HasValue && like.Value ? Modules.Log.Action.LikePost : Modules.Log.Action.DislikePost,
                    SubjectID = postId,
                    ModuleIdentifier = ModuleIdentifier.SH
                });
            }
            //end of Save Log
        }

        protected void like_dislike_comment(Guid? commentId, bool? like, double? score, DateTime date, ref string responseText)
        {
            if (!paramsContainer.GBEdit) return;

            LikeDislike info = new LikeDislike()
            {
                ObjectID = commentId,
                UserID = paramsContainer.CurrentUserID,
                Like = like,
                Score = score,
                Date = date
            };

            bool result = commentId.HasValue && SharingController.like_dislike_comment(paramsContainer.Tenant.Id, info);

            responseText = result ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully.ToString() + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed.ToString() + "\"}";

            //Send Notification
            if (result)
            {
                Notification not = new Notification()
                {
                    SubjectID = commentId,
                    SubjectType = SubjectType.Comment,
                    Action = like.HasValue && like.Value ?
                        Modules.NotificationCenter.ActionType.Like : Modules.NotificationCenter.ActionType.Dislike
                };
                not.Sender.UserID = paramsContainer.CurrentUserID;
                NotificationController.send_notification(paramsContainer.Tenant.Id, not);

                not = new Notification()
                {
                    SubjectID = commentId,
                    Action = like.HasValue && like.Value ?
                        Modules.NotificationCenter.ActionType.Dislike : Modules.NotificationCenter.ActionType.Like
                };
                not.Sender.UserID = paramsContainer.CurrentUserID;
                NotificationController.remove_notifications(paramsContainer.Tenant.Id, not);
            }
            //end of Send Notification

            //Save Log
            if (result)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = date,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = like.HasValue && like.Value ? Modules.Log.Action.LikeComment : Modules.Log.Action.DislikeComment,
                    SubjectID = commentId,
                    ModuleIdentifier = ModuleIdentifier.SH
                });
            }
            //end of Save Log
        }

        protected void unlike(Guid? objectId, bool post, ref string responseText)
        {
            if (!paramsContainer.GBEdit) return;

            LikeDislike info = new LikeDislike()
            {
                ObjectID = objectId,
                UserID = paramsContainer.CurrentUserID
            };

            bool result = post ? SharingController.unlike_post(paramsContainer.Tenant.Id, info) :
                SharingController.unlike_comment(paramsContainer.Tenant.Id, info);

            responseText = result ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully.ToString() + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed.ToString() + "\"}";

            //Remove Notifications
            if (result)
            {
                Notification not = new Notification();

                not.SubjectID = objectId;
                not.Sender.UserID = paramsContainer.CurrentUserID;
                List<string> actions = new List<string>();
                actions.Add(Modules.NotificationCenter.ActionType.Like.ToString());
                actions.Add(Modules.NotificationCenter.ActionType.Dislike.ToString());

                NotificationController.remove_notifications(paramsContainer.Tenant.Id, not, actions);
            }
            //end of Remove Notifications

            //Save Log
            if (result)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = DateTime.Now,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = post ? Modules.Log.Action.UnlikePost : Modules.Log.Action.UnlikeComment,
                    SubjectID = objectId,
                    ModuleIdentifier = ModuleIdentifier.SH
                });
            }
            //end of Save Log
        }

        public void get_fans(Guid itemId, bool isPost, bool? likeStatus, int? count, long? lowerBoundary, ref string responseText)
        {
            if (!paramsContainer.GBView) return;

            long totalCount = 0;

            List<User> users = isPost ? SharingController.get_post_fans(paramsContainer.Tenant.Id,
                itemId, likeStatus, count, lowerBoundary, ref totalCount) :
                SharingController.get_comment_fans(paramsContainer.Tenant.Id,
                itemId, likeStatus, count, lowerBoundary, ref totalCount);

            responseText = "{\"TotalCount\":" + totalCount.ToString() +
                ",\"Users\":[" + ProviderUtil.list_to_string<string>(users.Select(
                    u => "{\"UserID\":\"" + u.UserID.ToString() + "\"" +
                        ",\"UserName\":\"" + Base64.encode(u.UserName) + "\"" +
                        ",\"FirstName\":\"" + Base64.encode(u.FirstName) + "\"" +
                        ",\"LastName\":\"" + Base64.encode(u.LastName) + "\"" +
                        ",\"ProfileImageURL\":\"" + DocumentUtilities.get_personal_image_address(
                            paramsContainer.Tenant.Id, u.UserID.Value) + "\"" +
                        "}").ToList()) + "]" +
                "}";
        }

        protected void get_posts_count(Guid? ownerId, Guid? senderUserId, ref string responseText)
        {
            if (!paramsContainer.GBView) return;

            long count = SharingController.get_posts_count(paramsContainer.Tenant.Id, ownerId, senderUserId);

            responseText = "{\"Count\":\"" + count.ToString() + "\"}";
        }

        protected void get_shares_count(Guid? postId, ref string responseText)
        {
            if (!paramsContainer.GBView) return;

            long count = SharingController.get_shares_count(paramsContainer.Tenant.Id, postId);

            responseText = "{\"Count\":\"" + count.ToString() + "\"}";
        }

        protected void get_comments_count(Guid? postId, Guid? senderUserId, ref string responseText)
        {
            if (!paramsContainer.GBView) return;

            long count = SharingController.get_comments_count(paramsContainer.Tenant.Id, postId, senderUserId);

            responseText = "{\"Count\":\"" + count.ToString() + "\"}";
        }

        protected void get_user_posts_count(Guid? userId, int postTypeId, ref string responseText)
        {
            if (!paramsContainer.GBView) return;

            long count = SharingController.get_user_posts_count(paramsContainer.Tenant.Id, userId, postTypeId);

            responseText = "{\"Count\":\"" + count.ToString() + "\"}";
        }

        protected void get_post_likes_dislikes_count(Guid? postId, bool? like, ref string responseText)
        {
            if (!paramsContainer.GBView) return;

            long count = SharingController.get_post_likes_dislikes_count(paramsContainer.Tenant.Id,
                postId, like.HasValue && like.Value);

            responseText = "{\"Count\":\"" + count.ToString() + "\"}";
        }

        protected void get_comment_likes_dislikes_count(Guid? commentId, bool? like, ref string responseText)
        {
            if (!paramsContainer.GBView) return;

            long count = SharingController.get_comment_likes_dislikes_count(paramsContainer.Tenant.Id,
                commentId, like.HasValue && like.Value);

            responseText = "{\"Count\":\"" + count.ToString() + "\"}";
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}