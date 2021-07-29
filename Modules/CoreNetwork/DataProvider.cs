using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.Users;
using RaaiVan.Modules.Log;
using RaaiVan.Modules.FormGenerator;
using System.Collections;

namespace RaaiVan.Modules.CoreNetwork
{
    class DataProvider
    {
        public static string GetFullyQualifiedName(string name)
        {
            return "[dbo]." + "[CN_" + name + "]"; //'[dbo].' is database owner and 'CN_' is module qualifier
        }

        private static long _parse_node_types(ref IDataReader reader, ref List<NodeType> lstNodeTypes)
        {
            while (reader.Read())
            {
                try
                {
                    NodeType nodeType = new NodeType();

                    if (!string.IsNullOrEmpty(reader["NodeTypeID"].ToString())) nodeType.NodeTypeID = (Guid)reader["NodeTypeID"];
                    if (!string.IsNullOrEmpty(reader["ParentID"].ToString())) nodeType.ParentID = (Guid)reader["ParentID"];
                    if (!string.IsNullOrEmpty(reader["Name"].ToString())) nodeType.Name = (string)reader["Name"];
                    if (!string.IsNullOrEmpty(reader["AdditionalID"].ToString()))
                    {
                        nodeType.NodeTypeAdditionalID = (string)reader["AdditionalID"];

                        try { nodeType.AdditionalID = int.Parse((string)reader["AdditionalID"]); }
                        catch { }
                    }
                    if (string.IsNullOrEmpty(reader["AdditionalIDPattern"].ToString()))
                    {
                        nodeType.AdditionalIDPattern = CNUtilities.DefaultAdditionalIDPattern;
                        nodeType.HasDefaultPattern = true;
                    }
                    else
                    {
                        nodeType.AdditionalIDPattern = (string)reader["AdditionalIDPattern"];
                        nodeType.HasDefaultPattern = false;
                    }
                    if (!string.IsNullOrEmpty(reader["Archive"].ToString())) nodeType.Archive = (bool)reader["Archive"];
                    if (!string.IsNullOrEmpty(reader["IsService"].ToString())) nodeType.IsService = (bool)reader["IsService"];

                    lstNodeTypes.Add(nodeType);
                }
                catch { }
            }

            long totalCount = (reader.NextResult()) ? ProviderUtil.succeed_long(reader) : 0;

            if (!reader.IsClosed) reader.Close();

            return totalCount;
        }

        private static long _parse_nodes(ref IDataReader reader, ref List<Node> lstNodes, 
            ref List<NodesCount> lstCounts, bool? full, bool hasTotalCount = false, bool fetchCounts = false)
        {
            while (reader.Read())
            {
                try
                {
                    Node node = new Node();

                    if (!string.IsNullOrEmpty(reader["NodeID"].ToString())) node.NodeID = (Guid)reader["NodeID"];
                    if (!string.IsNullOrEmpty(reader["NodeTypeID"].ToString())) node.NodeTypeID = (Guid)reader["NodeTypeID"];
                    if (!string.IsNullOrEmpty(reader["NodeType"].ToString())) node.NodeType = (string)reader["NodeType"];
                    if (!string.IsNullOrEmpty(reader["NodeTypeAdditionalID"].ToString()))
                        node.TypeAdditionalID = (string)reader["NodeTypeAdditionalID"];
                    if (!string.IsNullOrEmpty(reader["NodeTypeAdditionalID"].ToString()))
                    {
                        try { node.NodeTypeAdditionalID = (int.Parse((string)reader["NodeTypeAdditionalID"])); }
                        catch { }
                    }
                    if (!string.IsNullOrEmpty(reader["Name"].ToString())) node.Name = (string)reader["Name"];
                    if (!string.IsNullOrEmpty(reader["AdditionalID_Main"].ToString()))
                        node.AdditionalID_Main = (string)reader["AdditionalID_Main"];
                    if (!string.IsNullOrEmpty(reader["AdditionalID"].ToString()))
                        node.AdditionalID = (string)reader["AdditionalID"];
                    if (!string.IsNullOrEmpty(reader["ParentNodeID"].ToString())) node.ParentNodeID = (Guid)reader["ParentNodeID"];
                    if (!string.IsNullOrEmpty(reader["CreatorUserID"].ToString())) node.Creator.UserID = (Guid)reader["CreatorUserID"];
                    if (!string.IsNullOrEmpty(reader["CreationDate"].ToString())) node.CreationDate = (DateTime)reader["CreationDate"];
                    if (!string.IsNullOrEmpty(reader["AdminAreaID"].ToString())) node.AdminAreaID = (Guid)reader["AdminAreaID"];
                    if (!string.IsNullOrEmpty(reader["DocumentTreeNodeID"].ToString())) node.DocumentTreeNodeID = (Guid)reader["DocumentTreeNodeID"];

                    Status st = Status.NotSet;
                    if (Enum.TryParse<Status>(reader["Status"].ToString(), out st)) node.Status = st;

                    if (!string.IsNullOrEmpty(reader["WFState"].ToString())) node.WFState = (string)reader["WFState"];
                    if (!string.IsNullOrEmpty(reader["Searchable"].ToString())) node.Searchable = (bool)reader["Searchable"];
                    if (!string.IsNullOrEmpty(reader["HideCreators"].ToString())) node.HideCreators = (bool)reader["HideCreators"];
                    if (!string.IsNullOrEmpty(reader["Archive"].ToString())) node.Archive = (bool)reader["Archive"];

                    if (full.HasValue && full.Value)
                    {
                        if (!string.IsNullOrEmpty(reader["CreatorUserName"].ToString()))
                            node.Creator.UserName = (string)reader["CreatorUserName"];
                        if (!string.IsNullOrEmpty(reader["CreatorFirstName"].ToString()))
                            node.Creator.FirstName = (string)reader["CreatorFirstName"];
                        if (!string.IsNullOrEmpty(reader["CreatorLastName"].ToString()))
                            node.Creator.LastName = (string)reader["CreatorLastName"];
                        if (!string.IsNullOrEmpty(reader["DocumentTreeID"].ToString()))
                            node.DocumentTreeID = (Guid)reader["DocumentTreeID"];
                        if (!string.IsNullOrEmpty(reader["DocumentTreeName"].ToString()))
                            node.DocumentTreeName = (string)reader["DocumentTreeName"];
                        if (!string.IsNullOrEmpty(reader["PreviousVersionID"].ToString()))
                            node.PreviousVersionID = (Guid)reader["PreviousVersionID"];
                        if (!string.IsNullOrEmpty(reader["PreviousVersionName"].ToString()))
                            node.PreviousVersionName = (string)reader["PreviousVersionName"];
                        if (!string.IsNullOrEmpty(reader["Description"].ToString()))
                            node.Description = (string)reader["Description"];
                        if (!string.IsNullOrEmpty(reader["PublicDescription"].ToString()))
                            node.PublicDescription = (string)reader["PublicDescription"];
                        if (!string.IsNullOrEmpty(reader["Tags"].ToString()))
                        {
                            string strTags = strTags = (string)reader["Tags"];
                            node.Tags = ProviderUtil.get_tags_list(strTags);
                        }
                        if (!string.IsNullOrEmpty(reader["LikesCount"].ToString()))
                            node.LikesCount = (int)reader["LikesCount"];
                        if (!string.IsNullOrEmpty(reader["LikeStatus"].ToString()))
                            node.LikeStatus = (bool)reader["LikeStatus"];
                        if (!string.IsNullOrEmpty(reader["MembershipStatus"].ToString()))
                            node.MembershipStatus = (string)reader["MembershipStatus"];
                        if (!string.IsNullOrEmpty(reader["VisitsCount"].ToString()))
                            node.VisitsCount = (int)reader["VisitsCount"];
                        if (!string.IsNullOrEmpty(reader["AdminAreaName"].ToString()))
                            node.AdminAreaName = (string)reader["AdminAreaName"];
                        if (!string.IsNullOrEmpty(reader["AdminAreaType"].ToString()))
                            node.AdminAreaType = (string)reader["AdminAreaType"];
                        if (!string.IsNullOrEmpty(reader["ConfidentialityLevelID"].ToString()))
                            node.ConfidentialityLevel.ID = (Guid)reader["ConfidentialityLevelID"];
                        if (!string.IsNullOrEmpty(reader["ConfidentialityLevelNum"].ToString()))
                            node.ConfidentialityLevel.LevelID = (int)reader["ConfidentialityLevelNum"];
                        if (!string.IsNullOrEmpty(reader["ConfidentialityLevel"].ToString()))
                            node.ConfidentialityLevel.Title = (string)reader["ConfidentialityLevel"];
                        if (!string.IsNullOrEmpty(reader["OwnerID"].ToString()))
                            node.OwnerID = (Guid)reader["OwnerID"];
                        if (!string.IsNullOrEmpty(reader["OwnerName"].ToString()))
                            node.OwnerName = (string)reader["OwnerName"];

                        if (!string.IsNullOrEmpty(reader["PublicationDate"].ToString()))
                            node.PublicationDate = (DateTime)reader["PublicationDate"];
                        if (!string.IsNullOrEmpty(reader["ExpirationDate"].ToString()))
                            node.ExpirationDate = (DateTime)reader["ExpirationDate"];
                        if (!string.IsNullOrEmpty(reader["Score"].ToString()))
                            node.Score = (double)reader["Score"];
                        if (!string.IsNullOrEmpty(reader["IsFreeUser"].ToString()))
                            node.IsFreeUser = (bool)reader["IsFreeUser"];
                        if (!string.IsNullOrEmpty(reader["HasWikiContent"].ToString()))
                            node.HasWikiContent = (bool)reader["HasWikiContent"];
                        if (!string.IsNullOrEmpty(reader["HasFormContent"].ToString()))
                            node.HasFormContent = (bool)reader["HasFormContent"];
                    }

                    lstNodes.Add(node);
                }
                catch (Exception ex)
                {
                    string strEx = ex.ToString();
                }
            }

            long totalCount = (hasTotalCount && reader.NextResult()) ? ProviderUtil.succeed_long(reader, dontClose: true) : 0;

            if (fetchCounts && reader.NextResult()) _parse_nodes_count(ref reader, ref lstCounts);

            if (!reader.IsClosed) reader.Close();

            return totalCount;
        }

        private static long _parse_nodes(ref IDataReader reader, ref List<Node> lstNodes, bool? full, bool hasTotalCount = false)
        {
            List<NodesCount> lst = new List<NodesCount>();
            return _parse_nodes(ref reader, ref lstNodes, ref lst, full, hasTotalCount, fetchCounts: false);
        }

        private static void _parse_nodes_count(ref IDataReader reader, ref List<NodesCount> lstNodesCount)
        {
            bool? hasOrder = null;

            while (reader.Read())
            {
                try
                {
                    NodesCount nodesCount = new NodesCount();

                    if (!hasOrder.HasValue || hasOrder.Value)
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(reader["Order"].ToString())) nodesCount.Order = (int)reader["Order"];
                            if (!string.IsNullOrEmpty(reader["ReverseOrder"].ToString())) nodesCount.ReverseOrder = (int)reader["ReverseOrder"];

                            hasOrder = true;
                        }
                        catch { hasOrder = false; }
                    }

                    if (!string.IsNullOrEmpty(reader["NodeTypeID"].ToString())) nodesCount.NodeTypeID = (Guid)reader["NodeTypeID"];
                    if (!string.IsNullOrEmpty(reader["NodeTypeAdditionalID"].ToString()))
                        nodesCount.NodeTypeAdditionalID = (string)reader["NodeTypeAdditionalID"];
                    if (!string.IsNullOrEmpty(reader["TypeName"].ToString())) nodesCount.TypeName = (string)reader["TypeName"];
                    if (!string.IsNullOrEmpty(reader["NodesCount"].ToString())) nodesCount.Count = (int)reader["NodesCount"];

                    lstNodesCount.Add(nodesCount);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static long _parse_experts(ref IDataReader reader, ref List<Expert> lstExperts)
        {
            long? totalCount = null;

            while (reader.Read())
            {
                try
                {
                    Expert expert = new Expert();

                    if (!totalCount.HasValue)
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(reader["TotalCount"].ToString())) totalCount = (long)reader["TotalCount"];
                        }
                        catch { totalCount = 0; }
                    }

                    if (!string.IsNullOrEmpty(reader["NodeID"].ToString())) expert.Node.NodeID = (Guid)reader["NodeID"];
                    if (!string.IsNullOrEmpty(reader["NodeAdditionalID"].ToString()))
                        expert.Node.AdditionalID = (string)reader["NodeAdditionalID"];
                    if (!string.IsNullOrEmpty(reader["NodeName"].ToString())) expert.Node.Name = (string)reader["NodeName"];
                    if (!string.IsNullOrEmpty(reader["NodeTypeID"].ToString())) expert.Node.NodeTypeID = (Guid)reader["NodeTypeID"];
                    if (!string.IsNullOrEmpty(reader["NodeType"].ToString())) expert.Node.NodeType = (string)reader["NodeType"];
                    if (!string.IsNullOrEmpty(reader["ExpertUserID"].ToString())) expert.User.UserID = (Guid)reader["ExpertUserID"];
                    if (!string.IsNullOrEmpty(reader["ExpertUserName"].ToString())) expert.User.UserName = (string)reader["ExpertUserName"];
                    if (!string.IsNullOrEmpty(reader["ExpertFirstName"].ToString())) expert.User.FirstName = (string)reader["ExpertFirstName"];
                    if (!string.IsNullOrEmpty(reader["ExpertLastName"].ToString())) expert.User.LastName = (string)reader["ExpertLastName"];

                    lstExperts.Add(expert);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();

            return totalCount.HasValue ? totalCount.Value : 0;
        }

        private static void _parse_expertise_suggestions(ref IDataReader reader, ref List<Expert> lstExperts)
        {
            while (reader.Read())
            {
                try
                {
                    Expert expert = new Expert();

                    if (!string.IsNullOrEmpty(reader["NodeID"].ToString())) expert.Node.NodeID = (Guid)reader["NodeID"];
                    if (!string.IsNullOrEmpty(reader["NodeName"].ToString())) expert.Node.Name = (string)reader["NodeName"];
                    if (!string.IsNullOrEmpty(reader["NodeType"].ToString())) expert.Node.NodeType = (string)reader["NodeType"];
                    if (!string.IsNullOrEmpty(reader["ExpertUserID"].ToString())) expert.User.UserID = (Guid)reader["ExpertUserID"];
                    if (!string.IsNullOrEmpty(reader["ExpertUserName"].ToString())) expert.User.UserName = (string)reader["ExpertUserName"];
                    if (!string.IsNullOrEmpty(reader["ExpertFirstName"].ToString())) expert.User.FirstName = (string)reader["ExpertFirstName"];
                    if (!string.IsNullOrEmpty(reader["ExpertLastName"].ToString())) expert.User.LastName = (string)reader["ExpertLastName"];

                    lstExperts.Add(expert);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_node_info(ref IDataReader reader, ref List<NodeInfo> retList)
        {
            while (reader.Read())
            {
                try
                {
                    NodeInfo info = new NodeInfo();

                    if (!string.IsNullOrEmpty(reader["NodeID"].ToString())) info.NodeID = (Guid)reader["NodeID"];
                    if (!string.IsNullOrEmpty(reader["NodeTypeID"].ToString())) info.NodeTypeID = (Guid)reader["NodeTypeID"];
                    if (!string.IsNullOrEmpty(reader["Description"].ToString())) info.Description = (string)reader["Description"];
                    if (!string.IsNullOrEmpty(reader["CreatorUserID"].ToString())) info.Creator.UserID = (Guid)reader["CreatorUserID"];
                    if (!string.IsNullOrEmpty(reader["CreatorUserName"].ToString())) info.Creator.UserName = (string)reader["CreatorUserName"];
                    if (!string.IsNullOrEmpty(reader["CreatorFirstName"].ToString())) info.Creator.FirstName = (string)reader["CreatorFirstName"];
                    if (!string.IsNullOrEmpty(reader["CreatorLastName"].ToString())) info.Creator.LastName = (string)reader["CreatorLastName"];
                    if (!string.IsNullOrEmpty(reader["ContributorsCount"].ToString())) info.ContributorsCount = (int)reader["ContributorsCount"];
                    if (!string.IsNullOrEmpty(reader["LikesCount"].ToString())) info.LikesCount = (int)reader["LikesCount"];
                    if (!string.IsNullOrEmpty(reader["VisitsCount"].ToString())) info.VisitsCount = (int)reader["VisitsCount"];
                    if (!string.IsNullOrEmpty(reader["ExpertsCount"].ToString())) info.ExpertsCount = (int)reader["ExpertsCount"];
                    if (!string.IsNullOrEmpty(reader["MembersCount"].ToString())) info.MembersCount = (int)reader["MembersCount"];
                    if (!string.IsNullOrEmpty(reader["ChildsCount"].ToString())) info.ChildsCount = (int)reader["ChildsCount"];
                    if (!string.IsNullOrEmpty(reader["RelatedNodesCount"].ToString())) info.RelatedNodesCount = (int)reader["RelatedNodesCount"];
                    if (!string.IsNullOrEmpty(reader["LikeStatus"].ToString())) info.LikeStatus = (bool)reader["LikeStatus"];

                    if (!string.IsNullOrEmpty(reader["Tags"].ToString()))
                    {
                        string strTags = strTags = (string)reader["Tags"];
                        info.Tags = ProviderUtil.get_tags_list(strTags);
                    }

                    retList.Add(info);
                }
                catch (Exception ex) { string strEx = ex.ToString(); }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_extensions(ref IDataReader reader, ref List<Extension> extensions)
        {
            while (reader.Read())
            {
                try
                {
                    Extension extension = new Extension();

                    if (!string.IsNullOrEmpty(reader["OwnerID"].ToString())) extension.OwnerID = (Guid)reader["OwnerID"];
                    if (!string.IsNullOrEmpty(reader["Title"].ToString())) extension.Title = (string)reader["Title"];
                    if (!string.IsNullOrEmpty(reader["Disabled"].ToString())) extension.Disabled = (bool)reader["Disabled"];

                    string strExtension = string.IsNullOrEmpty(reader["Extension"].ToString()) ? null : (string)reader["Extension"];
                    try
                    {
                        if (!string.IsNullOrEmpty(strExtension))
                            extension.ExtensionType = (ExtensionType)Enum.Parse(typeof(ExtensionType), strExtension);
                    }
                    catch { extension.ExtensionType = ExtensionType.NotSet; }

                    extension.Initialized = true;

                    if (extension.ExtensionType != ExtensionType.NotSet) extensions.Add(extension);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_services(ref IDataReader reader, ref List<Service> services)
        {
            while (reader.Read())
            {
                try
                {
                    Service service = new Service();

                    if (!string.IsNullOrEmpty(reader["NodeTypeID"].ToString()))
                        service.NodeType.NodeTypeID = (Guid)reader["NodeTypeID"];
                    if (!string.IsNullOrEmpty(reader["NodeType"].ToString()))
                        service.NodeType.Name = (string)reader["NodeType"];
                    if (!string.IsNullOrEmpty(reader["ServiceTitle"].ToString()))
                        service.Title = (string)reader["ServiceTitle"];
                    if (!string.IsNullOrEmpty(reader["ServiceDescription"].ToString()))
                        service.Description = (string)reader["ServiceDescription"];
                    if (!string.IsNullOrEmpty(reader["EnableContribution"].ToString()))
                        service.EnableContribution = (bool)reader["EnableContribution"];
                    if (!string.IsNullOrEmpty(reader["NoContent"].ToString()))
                        service.NoContent = (bool)reader["NoContent"];
                    if (!string.IsNullOrEmpty(reader["IsKnowledge"].ToString()))
                        service.IsKnowledge = (bool)reader["IsKnowledge"];
                    if (!string.IsNullOrEmpty(reader["IsDocument"].ToString()))
                        service.IsDocument = (bool)reader["IsDocument"];
                    if (!string.IsNullOrEmpty(reader["EnablePreviousVersionSelect"].ToString()))
                        service.EnablePreviousVersionSelect = (bool)reader["EnablePreviousVersionSelect"];
                    if (!string.IsNullOrEmpty(reader["IsTree"].ToString()))
                        service.IsTree = (bool)reader["IsTree"];
                    if (!string.IsNullOrEmpty(reader["UniqueMembership"].ToString()))
                        service.UniqueMembership = (bool)reader["UniqueMembership"];
                    if (!string.IsNullOrEmpty(reader["UniqueAdminMember"].ToString()))
                        service.UniqueAdminMember = (bool)reader["UniqueAdminMember"];
                    if (!string.IsNullOrEmpty(reader["DisableAbstractAndKeywords"].ToString()))
                        service.DisableAbstractAndKeywords = (bool)reader["DisableAbstractAndKeywords"];
                    if (!string.IsNullOrEmpty(reader["DisableFileUpload"].ToString()))
                        service.DisableFileUpload = (bool)reader["DisableFileUpload"];
                    if (!string.IsNullOrEmpty(reader["DisableRelatedNodesSelect"].ToString()))
                        service.DisableRelatedNodesSelect = (bool)reader["DisableRelatedNodesSelect"];
                    if (!string.IsNullOrEmpty(reader["AdminNodeID"].ToString()))
                        service.AdminNode.NodeID = (Guid)reader["AdminNodeID"];
                    if (!string.IsNullOrEmpty(reader["MaxAcceptableAdminLevel"].ToString()))
                        service.MaxAcceptableAdminLevel = (int)reader["MaxAcceptableAdminLevel"];
                    if (!string.IsNullOrEmpty(reader["LimitAttachedFilesTo"].ToString()))
                        service.LimitAttachedFilesTo = ListMaker.get_string_items((string)reader["LimitAttachedFilesTo"], ',');
                    if (!string.IsNullOrEmpty(reader["MaxAttachedFileSize"].ToString()))
                        service.MaxAttachedFileSize = (int)reader["MaxAttachedFileSize"];
                    if (!string.IsNullOrEmpty(reader["MaxAttachedFilesCount"].ToString()))
                        service.MaxAttachedFilesCount = (int)reader["MaxAttachedFilesCount"];
                    if (!string.IsNullOrEmpty(reader["EditableForAdmin"].ToString()))
                        service.EditableForAdmin = (bool)reader["EditableForAdmin"];
                    if (!string.IsNullOrEmpty(reader["EditableForCreator"].ToString()))
                        service.EditableForCreator = (bool)reader["EditableForCreator"];
                    if (!string.IsNullOrEmpty(reader["EditableForOwners"].ToString()))
                        service.EditableForContributors = (bool)reader["EditableForOwners"];
                    if (!string.IsNullOrEmpty(reader["EditableForExperts"].ToString()))
                        service.EditableForExperts = (bool)reader["EditableForExperts"];
                    if (!string.IsNullOrEmpty(reader["EditableForMembers"].ToString()))
                        service.EditableForMembers = (bool)reader["EditableForMembers"];
                    if (!string.IsNullOrEmpty(reader["EditSuggestion"].ToString()))
                        service.EditSuggestion = (bool)reader["EditSuggestion"];

                    string strAdminType = string.IsNullOrEmpty(reader["AdminType"].ToString()) ? null : (string)reader["AdminType"];
                    try
                    {
                        if (!string.IsNullOrEmpty(strAdminType))
                            service.AdminType = (ServiceAdminType)Enum.Parse(typeof(ServiceAdminType), strAdminType);
                    }
                    catch { service.AdminType = ServiceAdminType.NotSet; }

                    services.Add(service);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static long _parse_explore_items(ref IDataReader reader, ref List<ExploreItem> items)
        {
            long? totalCount = 0;

            while (reader.Read())
            {
                try
                {
                    ExploreItem itm = new CoreNetwork.ExploreItem();

                    if (!string.IsNullOrEmpty(reader["TotalCount"].ToString())) totalCount = (long)reader["TotalCount"];

                    if (!string.IsNullOrEmpty(reader["BaseID"].ToString())) itm.BaseID = (Guid)reader["BaseID"];
                    if (!string.IsNullOrEmpty(reader["BaseTypeID"].ToString())) itm.BaseTypeID = (Guid)reader["BaseTypeID"];
                    if (!string.IsNullOrEmpty(reader["BaseName"].ToString())) itm.BaseName = (string)reader["BaseName"];
                    if (!string.IsNullOrEmpty(reader["BaseType"].ToString())) itm.BaseType = (string)reader["BaseType"];
                    if (!string.IsNullOrEmpty(reader["RelatedID"].ToString())) itm.RelatedID = (Guid)reader["RelatedID"];
                    if (!string.IsNullOrEmpty(reader["RelatedTypeID"].ToString())) itm.RelatedTypeID = (Guid)reader["RelatedTypeID"];
                    if (!string.IsNullOrEmpty(reader["RelatedName"].ToString())) itm.RelatedName = (string)reader["RelatedName"];
                    if (!string.IsNullOrEmpty(reader["RelatedType"].ToString())) itm.RelatedType = (string)reader["RelatedType"];
                    if (!string.IsNullOrEmpty(reader["RelatedCreationDate"].ToString()))
                        itm.RelatedCreationDate = (DateTime)reader["RelatedCreationDate"];
                    if (!string.IsNullOrEmpty(reader["IsTag"].ToString())) itm.IsTag = (bool)reader["IsTag"];
                    if (!string.IsNullOrEmpty(reader["IsRelation"].ToString())) itm.IsRelation = (bool)reader["IsRelation"];
                    if (!string.IsNullOrEmpty(reader["IsRegistrationArea"].ToString()))
                        itm.IsRegistrationArea = (bool)reader["IsRegistrationArea"];

                    items.Add(itm);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();

            return !totalCount.HasValue ? 0 : totalCount.Value;
        }

        private static void _parse_similar_nodes(ref IDataReader reader, ref List<SimilarNode> items)
        {
            while (reader.Read())
            {
                try
                {
                    SimilarNode itm = new CoreNetwork.SimilarNode();

                    if (!string.IsNullOrEmpty(reader["NodeID"].ToString())) itm.Suggested.NodeID = (Guid)reader["NodeID"];
                    if (!string.IsNullOrEmpty(reader["Rank"].ToString())) itm.Rank = (double)reader["Rank"];
                    if (!string.IsNullOrEmpty(reader["Tags"].ToString())) itm.Tags = (bool)reader["Tags"];
                    if (!string.IsNullOrEmpty(reader["Favorites"].ToString())) itm.Favorites = (bool)reader["Favorites"];
                    if (!string.IsNullOrEmpty(reader["Relations"].ToString())) itm.Relations = (bool)reader["Relations"];
                    if (!string.IsNullOrEmpty(reader["Experts"].ToString())) itm.Experts = (bool)reader["Experts"];
                    
                    items.Add(itm);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_knowledgable_users(ref IDataReader reader, ref List<KnowledgableUser> items)
        {
            while (reader.Read())
            {
                try
                {
                    KnowledgableUser itm = new CoreNetwork.KnowledgableUser();

                    if (!string.IsNullOrEmpty(reader["UserID"].ToString())) itm.User.UserID = (Guid)reader["UserID"];
                    if (!string.IsNullOrEmpty(reader["Rank"].ToString())) itm.Rank = (double)reader["Rank"];
                    if (!string.IsNullOrEmpty(reader["Expert"].ToString())) itm.Expert = (bool)reader["Expert"];
                    if (!string.IsNullOrEmpty(reader["Contributor"].ToString())) itm.Contributor = (bool)reader["Contributor"];
                    if (!string.IsNullOrEmpty(reader["WikiEditor"].ToString())) itm.WikiEditor = (bool)reader["WikiEditor"];
                    if (!string.IsNullOrEmpty(reader["Member"].ToString())) itm.Member = (bool)reader["Member"];
                    if (!string.IsNullOrEmpty(reader["ExpertOfRelatedNode"].ToString()))
                        itm.ExpertOfRelatedNode = (bool)reader["ExpertOfRelatedNode"];
                    if (!string.IsNullOrEmpty(reader["ContributorOfRelatedNode"].ToString()))
                        itm.ContributorOfRelatedNode = (bool)reader["ContributorOfRelatedNode"];
                    if (!string.IsNullOrEmpty(reader["MemberOfRelatedNode"].ToString()))
                        itm.MemberOfRelatedNode = (bool)reader["MemberOfRelatedNode"];

                    items.Add(itm);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }


        public static void GetExpertiseDomains(Guid applicationId, ref List<Node> retItems, Guid userId,
            List<Guid> nodeTypeIds, Guid? nodeId, string additionalId, string searchText,
            DateTime? lowerDateLimit, DateTime? upperDateLimit, int? lowerBoundary, int? count, ref long totalCount)
        {
            string spName = GetFullyQualifiedName("GetExpertiseDomains");

            try
            {
                if (nodeId == Guid.Empty) nodeId = null;
                if (lowerBoundary.HasValue && lowerBoundary <= 0) lowerBoundary = null;
                if (count.HasValue && count <= 0) count = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, userId, string.Join(",", nodeTypeIds), ',',
                    nodeId, additionalId, ProviderUtil.get_search_text(searchText),
                    lowerDateLimit, upperDateLimit, lowerBoundary, count);
                totalCount = _parse_nodes(ref reader, ref retItems, null, hasTotalCount: true);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetExpertiseDomains(Guid applicationId, ref List<Expert> retExperts, 
            ref List<Guid> userIds, Guid? nodeTypeId, bool? approved, bool? socialApproved, bool? all)
        {
            string spName = GetFullyQualifiedName("GetUsersExpertiseDomains");

            try
            {
                if (nodeTypeId == Guid.Empty) nodeTypeId = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref userIds), ',', nodeTypeId, approved, socialApproved, all);
                _parse_experts(ref reader, ref retExperts);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetExpertiseDomainIDs(Guid applicationId, 
            ref List<Guid> retIds, ref List<Guid> userIds, bool? approved, bool? socialApproved)
        {
            string spName = GetFullyQualifiedName("GetUsersExpertiseDomainIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref userIds), ',', approved, socialApproved);
                ProviderUtil.parse_guids(ref reader, ref retIds);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetExpertiseSuggestions(Guid applicationId, 
            ref List<Expert> retExperts, Guid userId, int count, int? lowerBoundary)
        {
            string spName = GetFullyQualifiedName("GetExpertiseSuggestions");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, userId, count, lowerBoundary);
                _parse_expertise_suggestions(ref reader, ref retExperts);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void SuggestNodeRelations(Guid applicationId, 
            ref List<Node> retNodes, Guid userId, Guid? relatedNodeTypeId, int? count)
        {
            string spName = GetFullyQualifiedName("SuggestNodeRelations");

            try
            {
                if (relatedNodeTypeId == Guid.Empty) relatedNodeTypeId = null;
                if (!count.HasValue) count = 20;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    userId, null, relatedNodeTypeId, count, DateTime.Now);
                _parse_nodes(ref reader, ref retNodes, null);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void SuggestNodeTypesForRelations(Guid applicationId, 
            ref List<NodeType> retNodeTypes, Guid userId, int? count)
        {
            string spName = GetFullyQualifiedName("SuggestNodeTypesForRelations");

            try
            {
                if (!count.HasValue) count = 10;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    userId, null, count, DateTime.Now);
                _parse_node_types(ref reader, ref retNodeTypes);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void SuggestSimilarNodes(Guid applicationId,
            ref List<SimilarNode> ret, Guid nodeId, int? count)
        {
            string spName = GetFullyQualifiedName("SuggestSimilarNodes");

            try
            {
                if (count.HasValue && count <= 0) count = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, nodeId, count);
                _parse_similar_nodes(ref reader, ref ret);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void SuggestKnowledgableUsers(Guid applicationId,
            ref List<KnowledgableUser> ret, Guid nodeId, int? count)
        {
            string spName = GetFullyQualifiedName("SuggestKnowledgableUsers");

            try
            {
                if (count.HasValue && count <= 0) count = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, nodeId, count);
                _parse_knowledgable_users(ref reader, ref ret);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetExistingNodeIDs(Guid applicationId, 
            ref List<Guid> retIds, ref List<Guid> nodeIds, bool? searchable, bool? noContent)
        {
            string spName = GetFullyQualifiedName("GetExistingNodeIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string(ref nodeIds), ',', searchable, noContent);
                ProviderUtil.parse_guids(ref reader, ref retIds);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetExistingNodeTypeIDs(Guid applicationId,
            ref List<Guid> retIds, ref List<Guid> nodeTypeIds, bool? noContent)
        {
            string spName = GetFullyQualifiedName("GetExistingNodeTypeIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string(ref nodeTypeIds), ',', noContent);
                ProviderUtil.parse_guids(ref reader, ref retIds);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetNodeInfo(Guid applicationId, ref List<NodeInfo> retList, List<Guid> nodeIds, 
            Guid? currentUserId, bool? tags, bool? description, bool? creator, bool? contributorsCount, bool? likesCount, 
            bool? visitsCount, bool? expertsCount, bool? membersCount, bool? childsCount, 
            bool? relatedNodesCount, bool? likeStatus)
        {
            string spName = GetFullyQualifiedName("GetNodeInfo");

            try
            {
                if (currentUserId == Guid.Empty) currentUserId = null;
                if (!currentUserId.HasValue) likeStatus = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(nodeIds), ',', currentUserId, tags, description, 
                    creator, contributorsCount, likesCount, visitsCount, expertsCount, membersCount, 
                    childsCount, relatedNodesCount, likeStatus);
                _parse_node_info(ref reader, ref retList);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static bool SaveExtensions(Guid applicationId, Guid ownerId, List<Extension> extensions, Guid currentUserId)
        {
            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //Add Extensions
            DataTable extensionsTable = new DataTable();
            extensionsTable.Columns.Add("OwnerID", typeof(Guid));
            extensionsTable.Columns.Add("Extension", typeof(string));
            extensionsTable.Columns.Add("Title", typeof(string));
            extensionsTable.Columns.Add("SequenceNumber", typeof(int));
            extensionsTable.Columns.Add("Disabled", typeof(bool));

            int seq = 1;

            extensions.ForEach(ex => extensionsTable.Rows.Add(null, ex.ExtensionType.ToString(), ex.Title, seq++, ex.Disabled));

            SqlParameter extensionsParam = new SqlParameter("@Extensions", SqlDbType.Structured);
            extensionsParam.TypeName = "[dbo].[CNExtensionTableType]";
            extensionsParam.Value = extensionsTable;
            //end of Add Extensions

            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.AddWithValue("@OwnerID", ownerId);
            cmd.Parameters.Add(extensionsParam);
            cmd.Parameters.AddWithValue("@CurrentUserID", currentUserId);
            cmd.Parameters.AddWithValue("@Now", DateTime.Now);

            string spName = GetFullyQualifiedName("SaveExtensions");

            string sep = ", ";
            string arguments = "@ApplicationID" + sep + "@OwnerID" + sep + 
                "@Extensions" + sep + "@CurrentUserID" + sep + "@Now";
            cmd.CommandText = ("EXEC" + " " + spName + " " + arguments);

            con.Open();
            try { return ProviderUtil.succeed((IDataReader)cmd.ExecuteReader()); }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
            finally { con.Close(); }
        }

        public static void GetExtensions(Guid applicationId, ref List<Extension> retExtensions, Guid ownerId)
        {
            string spName = GetFullyQualifiedName("GetExtensions");

            try
            {
                if (ownerId == Guid.Empty) return;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, ownerId);
                _parse_extensions(ref reader, ref retExtensions);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetNodeTypesWithExtension(Guid applicationId, 
            ref List<NodeType> retItems, List< ExtensionType> exts)
        {
            string spName = GetFullyQualifiedName("GetNodeTypesWithExtension");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, 
                    ProviderUtil.list_to_string<ExtensionType>(exts), ',');
                _parse_node_types(ref reader, ref retItems);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetIntellectualPropertiesCount(Guid applicationId, ref List<NodesCount> retItems, 
            Guid userId, Guid? nodeTypeId, Guid? nodeId , string additionalId, Guid? currentUserId, bool? isDocument,
            DateTime? lowerDateLimit, DateTime? upperDateLimit)
        {
            string spName = GetFullyQualifiedName("GetIntellectualPropertiesCount");

            try
            {
                if (nodeTypeId == Guid.Empty) nodeTypeId = null;
                if (nodeId == Guid.Empty) nodeId = null;
                if (currentUserId == Guid.Empty) currentUserId = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, userId, nodeTypeId,
                    nodeId, additionalId, currentUserId, isDocument, lowerDateLimit, upperDateLimit);
                _parse_nodes_count(ref reader, ref retItems);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetIntellectualProperties(Guid applicationId, ref List<Node> retItems, Guid userId,
            List<Guid> nodeTypeIds, Guid? nodeId, string additionalId, Guid? currentUserId, string searchText, bool? isDocument,
            DateTime? lowerDateLimit, DateTime? upperDateLimit, int? lowerBoundary, int? count, ref long totalCount)
        {
            string spName = GetFullyQualifiedName("GetIntellectualProperties");

            try
            {
                if (nodeId == Guid.Empty) nodeId = null;
                if (currentUserId == Guid.Empty) currentUserId = null;
                if (lowerBoundary.HasValue && lowerBoundary <= 0) lowerBoundary = null;
                if (count.HasValue && count <= 0) count = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, userId, string.Join(",", nodeTypeIds), ',', 
                    nodeId, additionalId, currentUserId, ProviderUtil.get_search_text(searchText),
                    isDocument, lowerDateLimit, upperDateLimit, lowerBoundary, count);
                totalCount = _parse_nodes(ref reader, ref retItems, null, hasTotalCount: true);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetIntellectualPropertiesOfFriends(Guid applicationId, ref List<Node> retItems, 
            Guid userId, Guid? nodeTypeId, int? lowerBoundary, int? count)
        {
            string spName = GetFullyQualifiedName("GetIntellectualPropertiesOfFriends");

            try
            {
                if (nodeTypeId == Guid.Empty) nodeTypeId = null;
                if (lowerBoundary.HasValue && lowerBoundary <= 0) lowerBoundary = null;
                if (count.HasValue && count <= 0) count = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    userId, nodeTypeId, lowerBoundary, count);
                _parse_nodes(ref reader, ref retItems, null);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetDocumentTreeNodeItems(Guid applicationId, ref List<Node> retItems, 
            Guid documentTreeNodeId, Guid? currenrUserId, bool? checkPrivacy, int? count, int? lowerBoundary)
        {
            string spName = GetFullyQualifiedName("GetDocumentTreeNodeItems");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, documentTreeNodeId, 
                    currenrUserId, checkPrivacy, DateTime.Now, RaaiVanSettings.DefaultPrivacy(applicationId), count, lowerBoundary);
                _parse_nodes(ref reader, ref retItems, null);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetDocumentTreeNodeContents(Guid applicationId, ref List<Node> retItems, 
            Guid documentTreeNodeId, Guid? currenrUserId, bool? checkPrivacy, int? count, int? lowerBoundary, 
            string searchText, ref long totalCount)
        {
            string spName = GetFullyQualifiedName("GetDocumentTreeNodeContents");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, documentTreeNodeId,
                    currenrUserId, checkPrivacy, DateTime.Now, RaaiVanSettings.DefaultPrivacy(applicationId), 
                    count, lowerBoundary, ProviderUtil.get_search_text(searchText));
                totalCount = _parse_nodes(ref reader, ref retItems, null, hasTotalCount: true);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static List<Guid> IsNodeType(Guid applicationId, List<Guid> ids)
        {
            string spName = GetFullyQualifiedName("IsNodeType");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ids), ',');
                List<Guid> ret = new List<Guid>();
                ProviderUtil.parse_guids(ref reader, ref ret);
                return ret;
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return new List<Guid>();
            }
        }

        public static List<Guid> IsNode(Guid applicationId, List<Guid> ids)
        {
            string spName = GetFullyQualifiedName("IsNode");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ids), ',');
                List<Guid> ret = new List<Guid>();
                ProviderUtil.parse_guids(ref reader, ref ret);
                return ret;
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return new List<Guid>();
            }
        }

        public static void Explore(Guid applicationId, ref List<ExploreItem> retItems, Guid? baseId, Guid? relatedId, 
            List<Guid> baseTypeIds, List<Guid> relatedTypeIds, Guid? secondLevelNodeId, 
            bool? registrationArea, bool? tags, bool? relations, int? lowerBoundary, int? count, string orderBy, 
            bool? orderByDesc, string searchText, bool? checkAccess, Guid? currentUserId, ref long totalCount)
        {
            string spName = GetFullyQualifiedName("Explore");

            try
            {
                if (baseId == Guid.Empty) baseId = null;
                if (relatedId == Guid.Empty) relatedId = null;
                if (secondLevelNodeId == Guid.Empty) secondLevelNodeId = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, baseId, relatedId,
                    ProviderUtil.list_to_string<Guid>(baseTypeIds), ProviderUtil.list_to_string<Guid>(relatedTypeIds), ',',
                    secondLevelNodeId, registrationArea, tags, relations, lowerBoundary, count, orderBy, orderByDesc,
                    ProviderUtil.get_search_text(searchText), checkAccess, currentUserId,
                    DateTime.Now, RaaiVanSettings.DefaultPrivacy(applicationId));
                totalCount = _parse_explore_items(ref reader, ref retItems);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        /* Service */

        public static void GetServices(Guid applicationId, ref List<Service> retServices, List<Guid> nodeTypeIds)
        {
            string spName = GetFullyQualifiedName("GetServicesByIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, 
                    ProviderUtil.list_to_string<Guid>(nodeTypeIds), ',');
                _parse_services(ref reader, ref retServices);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetServices(Guid applicationId, ref List<Service> retServices, Guid? nodeTypeIdOrNodeId,
            Guid? currentUserId, bool? isDocument, bool? isKnowledge, bool? checkPrivacy)
        {
            string spName = GetFullyQualifiedName("GetServices");

            try
            {
                if (nodeTypeIdOrNodeId == Guid.Empty) nodeTypeIdOrNodeId = null;
                if (currentUserId == Guid.Empty) currentUserId = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, nodeTypeIdOrNodeId, 
                    currentUserId, isDocument, isKnowledge, checkPrivacy, DateTime.Now, RaaiVanSettings.DefaultPrivacy(applicationId));
                _parse_services(ref reader, ref retServices);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetAdminAreaLimits(Guid applicationId, 
            ref List<NodeType> retNodeTypes, Guid nodeTypeIdOrnodeId)
        {
            string spName = GetFullyQualifiedName("GetAdminAreaLimits");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, nodeTypeIdOrnodeId);
                _parse_node_types(ref reader, ref retNodeTypes);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetContributionLimits(Guid applicationId, 
            ref List<NodeType> retNodeTypes, Guid nodeTypeIdOrnodeId)
        {
            string spName = GetFullyQualifiedName("GetContributionLimits");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, nodeTypeIdOrnodeId);
                _parse_node_types(ref reader, ref retNodeTypes);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetFreeUsers(Guid applicationId, ref List<User> retUsers, Guid nodeTypeId)
        {
            string spName = GetFullyQualifiedName("GetFreeUserIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, nodeTypeId);
                List<Guid> userIds = new List<Guid>();
                ProviderUtil.parse_guids(ref reader, ref userIds);
                retUsers = UsersController.get_users(applicationId, userIds);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static void GetServiceAdmins(Guid applicationId, ref List<User> retUsers, Guid nodeTypeId)
        {
            string spName = GetFullyQualifiedName("GetServiceAdminIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, nodeTypeId);
                List<Guid> userIds = new List<Guid>();
                ProviderUtil.parse_guids(ref reader, ref userIds);
                retUsers = UsersController.get_users(applicationId, userIds);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
            }
        }

        public static bool RegisterNewNode(Guid applicationId, Node nodeObject, Guid? workflowId, Guid? formInstanceId,
            Guid? wfDirectorNodeId, Guid? wfDirectorUserId, ref List<Dashboard> dashboards, ref string message)
        {
            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            if (nodeObject.ParentNodeID == Guid.Empty) nodeObject.ParentNodeID = null;
            if (nodeObject.DocumentTreeNodeID == Guid.Empty) nodeObject.DocumentTreeNodeID = null;
            if (nodeObject.PreviousVersionID == Guid.Empty) nodeObject.PreviousVersionID = null;

            //Add CreatorUsers
            DataTable contributorsTable = new DataTable();
            contributorsTable.Columns.Add("FirstValue", typeof(Guid));
            contributorsTable.Columns.Add("SecondValue", typeof(double));

            foreach (NodeCreator _cnt in nodeObject.Contributors)
                contributorsTable.Rows.Add(_cnt.User.UserID, _cnt.CollaborationShare);

            SqlParameter contributorsParam = new SqlParameter("@Contributors", SqlDbType.Structured);
            contributorsParam.TypeName = "[dbo].[GuidFloatTableType]";
            contributorsParam.Value = contributorsTable;
            //end of Add CreatorUsers

            if (!nodeObject.CreationDate.HasValue) nodeObject.CreationDate = DateTime.Now;

            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.AddWithValue("@NodeID", nodeObject.NodeID);
            cmd.Parameters.AddWithValue("@NodeTypeID", nodeObject.NodeTypeID);
            if (!string.IsNullOrEmpty(nodeObject.AdditionalID_Main))
                cmd.Parameters.AddWithValue("@AdditionalID_Main", nodeObject.AdditionalID_Main);
            if (!string.IsNullOrEmpty(nodeObject.AdditionalID))
                cmd.Parameters.AddWithValue("@AdditionalID", nodeObject.AdditionalID);
            if (nodeObject.ParentNodeID.HasValue)
                cmd.Parameters.AddWithValue("@ParentNodeID", nodeObject.ParentNodeID.Value);
            if (nodeObject.DocumentTreeNodeID.HasValue)
                cmd.Parameters.AddWithValue("@DocumentTreeNodeID", nodeObject.DocumentTreeNodeID.Value);
            if (nodeObject.PreviousVersionID.HasValue)
                cmd.Parameters.AddWithValue("@PreviousVersionID", nodeObject.PreviousVersionID.Value);
            cmd.Parameters.AddWithValue("@Name", nodeObject.Name);
            if(!string.IsNullOrEmpty(nodeObject.Description))
                cmd.Parameters.AddWithValue("@Description", nodeObject.Description);
            if (nodeObject.Tags.Count > 0) cmd.Parameters.AddWithValue("@Tags", ProviderUtil.get_tags_text(nodeObject.Tags));
            cmd.Parameters.AddWithValue("@CreatorUserID", nodeObject.Creator.UserID);
            cmd.Parameters.AddWithValue("@CreationDate", nodeObject.CreationDate);
            cmd.Parameters.Add(contributorsParam);
            if (nodeObject.OwnerID.HasValue) cmd.Parameters.AddWithValue("@OwnerID", nodeObject.OwnerID);
            if (workflowId.HasValue && workflowId != Guid.Empty)
                cmd.Parameters.AddWithValue("@WorkFlowID", workflowId.Value);
            if (nodeObject.AdminAreaID.HasValue && nodeObject.AdminAreaID != Guid.Empty)
                cmd.Parameters.AddWithValue("@AdminAreaID", nodeObject.AdminAreaID.Value);
            if (formInstanceId.HasValue && formInstanceId != Guid.Empty)
                cmd.Parameters.AddWithValue("@FormInstanceID", formInstanceId.Value);
            if (wfDirectorNodeId.HasValue && wfDirectorNodeId != Guid.Empty)
                cmd.Parameters.AddWithValue("@WFDirectorNodeID", wfDirectorNodeId.Value);
            if (wfDirectorUserId.HasValue && wfDirectorUserId != Guid.Empty)
                cmd.Parameters.AddWithValue("@WFDirectorUserID", wfDirectorUserId.Value);

            string spName = GetFullyQualifiedName("RegisterNewNode");

            string sep = ", ";
            string arguments = "@ApplicationID" + sep + "@NodeID" + sep + "@NodeTypeID" + sep +
                (string.IsNullOrEmpty(nodeObject.AdditionalID_Main) ? "null" : "@AdditionalID_Main") + sep +
                (string.IsNullOrEmpty(nodeObject.AdditionalID) ? "null" : "@AdditionalID") + sep +
                (!nodeObject.ParentNodeID.HasValue ? "null" : "@ParentNodeID") + sep +
                (!nodeObject.DocumentTreeNodeID.HasValue ? "null" : "@DocumentTreeNodeID") + sep +
                (!nodeObject.PreviousVersionID.HasValue ? "null" : "@PreviousVersionID") + sep +
                "@Name" + sep + 
                (string.IsNullOrEmpty(nodeObject.Description) ? "null" : "@Description") + sep +
                (nodeObject.Tags.Count == 0 ? "null" : "@Tags") + sep +
                "@CreatorUserID" + sep + "@CreationDate" + sep + "@Contributors" + sep +
                (nodeObject.OwnerID.HasValue ? "@OwnerID" : "null") + sep +
                (workflowId.HasValue && workflowId != Guid.Empty ? "@WorkFlowID" : "null") + sep +
                (nodeObject.AdminAreaID.HasValue && nodeObject.AdminAreaID != Guid.Empty ? "@AdminAreaID" : "null") + sep +
                (formInstanceId.HasValue && formInstanceId != Guid.Empty ? "@FormInstanceID" : "null") + sep +
                (wfDirectorNodeId.HasValue && wfDirectorNodeId != Guid.Empty ? "@WFDirectorNodeID" : "null") + sep +
                (wfDirectorUserId.HasValue && wfDirectorUserId != Guid.Empty ? "@WFDirectorUserID" : "null");
            cmd.CommandText = ("EXEC" + " " + spName + " " + arguments);

            con.Open();
            try
            {
                IDataReader reader = (IDataReader)cmd.ExecuteReader();
                return ProviderUtil.parse_dashboards(ref reader, ref dashboards, ref message) > 0;
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
            finally { con.Close(); }
        }

        public static bool SetContributors(Guid applicationId, Node nodeObject, ref string errorMessage)
        {
            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //Add CreatorUsers
            DataTable contributorsTable = new DataTable();
            contributorsTable.Columns.Add("FirstValue", typeof(Guid));
            contributorsTable.Columns.Add("SecondValue", typeof(double));

            foreach (NodeCreator _cnt in nodeObject.Contributors)
                contributorsTable.Rows.Add(_cnt.User.UserID, _cnt.CollaborationShare);

            SqlParameter contributorsParam = new SqlParameter("@Contributors", SqlDbType.Structured);
            contributorsParam.TypeName = "[dbo].[GuidFloatTableType]";
            contributorsParam.Value = contributorsTable;
            //end of Add CreatorUsers

            if (!nodeObject.LastModificationDate.HasValue) nodeObject.LastModificationDate = DateTime.Now;

            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.AddWithValue("@NodeID", nodeObject.NodeID);
            cmd.Parameters.Add(contributorsParam);
            if (nodeObject.OwnerID.HasValue) cmd.Parameters.AddWithValue("@OwnerID", nodeObject.OwnerID);
            cmd.Parameters.AddWithValue("@LastModifierUserID", nodeObject.LastModifierUserID);
            cmd.Parameters.AddWithValue("@LastModificationDate", nodeObject.LastModificationDate);

            string spName = GetFullyQualifiedName("SetContributors");

            string sep = ", ";
            string arguments = "@ApplicationID" + sep + "@NodeID" + sep + "@Contributors" + sep +
                (nodeObject.OwnerID.HasValue ? "@OwnerID" : "null") + sep +
                "@LastModifierUserID" + sep + "@LastModificationDate";
            cmd.CommandText = ("EXEC" + " " + spName + " " + arguments);

            con.Open();
            try { return ProviderUtil.succeed((IDataReader)cmd.ExecuteReader(), ref errorMessage); }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.CN);
                return false;
            }
            finally { con.Close(); }
        }

        /* end of Service */
    }
}
