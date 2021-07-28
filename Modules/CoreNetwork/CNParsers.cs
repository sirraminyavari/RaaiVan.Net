using RaaiVan.Modules.GlobalUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaaiVan.Modules.CoreNetwork
{
    public static class CNParsers
    {
        public static List<Node> nodes(DBResultSet results, ref List<NodesCount> lstCounts, 
            ref long totalCount, bool? full, bool fetchCounts = false)
        {
            List<Node> nodes = new List<Node>();

            RVDataTable table = results.get_table();

            for (int i = 0; i < table.Rows.Count; i++)
            {
                Node node = new Node();

                node.NodeID = table.GetGuid(i, "NodeID");
                node.NodeTypeID = table.GetGuid(i, "NodeTypeID");
                node.NodeType = table.GetString(i, "NodeType");
                node.TypeAdditionalID = table.GetString(i, "NodeTypeAdditionalID");
                node.NodeTypeAdditionalID = table.GetInt(i, "NodeTypeAdditionalID");
                node.Name = table.GetString(i, "Name");
                node.AdditionalID_Main = table.GetString(i, "AdditionalID_Main");
                node.AdditionalID = table.GetString(i, "AdditionalID");
                node.ParentNodeID = table.GetGuid(i, "ParentNodeID");
                node.Creator.UserID = table.GetGuid(i, "CreatorUserID");
                node.CreationDate = table.GetDate(i, "CreationDate");
                node.AdminAreaID = table.GetGuid(i, "AdminAreaID");
                node.DocumentTreeNodeID = table.GetGuid(i, "DocumentTreeNodeID");
                node.Status = table.GetEnum<Status>(i, "DocumentTreeNodeID", Status.NotSet);
                node.WFState = table.GetString(i, "WFState");
                node.Searchable = table.GetBool(i, "Searchable");
                node.HideCreators = table.GetBool(i, "HideCreators");
                node.Archive = table.GetBool(i, "Archive");
                
                if (full.HasValue && full.Value)
                {
                    node.Creator.UserName = table.GetString(i, "CreatorUserName");
                    node.Creator.FirstName = table.GetString(i, "CreatorFirstName");
                    node.Creator.LastName = table.GetString(i, "CreatorLastName");
                    node.DocumentTreeID = table.GetGuid(i, "DocumentTreeID");
                    node.DocumentTreeName = table.GetString(i, "DocumentTreeName");
                    node.PreviousVersionID = table.GetGuid(i, "PreviousVersionID");
                    node.PreviousVersionName = table.GetString(i, "PreviousVersionName");
                    node.Description = table.GetString(i, "Description");
                    node.PublicDescription = table.GetString(i, "PublicDescription");
                    node.Tags = ProviderUtil.get_tags_list(table.GetString(i, "Tags"));
                    node.LikesCount = table.GetInt(i, "LikesCount");
                    node.LikeStatus = table.GetBool(i, "LikeStatus");
                    node.MembershipStatus = table.GetString(i, "MembershipStatus");
                    node.VisitsCount = table.GetInt(i, "VisitsCount");
                    node.AdminAreaName = table.GetString(i, "AdminAreaName");
                    node.AdminAreaType = table.GetString(i, "AdminAreaType");
                    node.ConfidentialityLevel.ID = table.GetGuid(i, "ConfidentialityLevelID");
                    node.ConfidentialityLevel.LevelID = table.GetInt(i, "ConfidentialityLevelNum");
                    node.ConfidentialityLevel.Title = table.GetString(i, "ConfidentialityLevel");
                    node.OwnerID = table.GetGuid(i, "OwnerID");
                    node.OwnerName = table.GetString(i, "OwnerName");
                    node.PublicationDate = table.GetDate(i, "PublicationDate");
                    node.ExpirationDate = table.GetDate(i, "ExpirationDate");
                    node.Score = table.GetDouble(i, "Score");
                    node.IsFreeUser = table.GetBool(i, "IsFreeUser");
                    node.HasWikiContent = table.GetBool(i, "HasWikiContent");
                    node.HasFormContent = table.GetBool(i, "HasFormContent");

                }

                nodes.Add(node);
            }

            if (results.TablesCount > 1) totalCount = results.get_table(1).GetLong(row: 0, column: 0, defaultValue: 0).Value;

            if (fetchCounts && results.TablesCount > 2) lstCounts = nodes_count(results.get_table(2));

            return nodes;
        }

        public static List<NodesCount> nodes_count(RVDataTable table)
        {
            bool? hasOrder = null;

            List<NodesCount> lst = new List<NodesCount>();

            for (int i = 0; i < table.Rows.Count; i++)
            {
                NodesCount nodesCount = new NodesCount();

                if (!hasOrder.HasValue || hasOrder.Value)
                {
                    nodesCount.Order = table.GetInt(i, "Order");
                    nodesCount.ReverseOrder = table.GetInt(i, "ReverseOrder");

                    hasOrder = true;
                }

                nodesCount.NodeTypeID = table.GetGuid(i, "NodeTypeID");
                nodesCount.NodeTypeAdditionalID = table.GetString(i, "NodeTypeAdditionalID");
                nodesCount.TypeName = table.GetString(i, "TypeName");
                nodesCount.Count = table.GetInt(i, "NodesCount");

                lst.Add(nodesCount);
            }

            return lst;
        }

        public static List<NodesCount> nodes_count(DBResultSet results) {
            return nodes_count(results.get_table());
        }

        public static Dictionary<string, object> node_counts_grouped_by_element(DBResultSet results)
        {
            Dictionary<string, object> ret = new Dictionary<string, object>();
            ArrayList items = new ArrayList();

            RVDataTable table = results.get_table();

            ret["Items"] = items;

            for (int i = 0; i < table.Rows.Count; i++)
            {
                Dictionary<string, object> dic = new Dictionary<string, object>();

                dic["TextValue"] = Base64.encode(table.GetString(i, "TextValue"));
                dic["BitValue"] = table.GetValue(i, "BitValue");
                dic["Type"] = table.GetValue(i, "Type");
                dic["Count"] = table.GetValue(i, "Count");

                items.Add(dic);
            }

            return ret;
        }
    }
}
