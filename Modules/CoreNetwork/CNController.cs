using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.UI;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.Users;
using RaaiVan.Modules.FormGenerator;
using RaaiVan.Modules.Privacy;
using RaaiVan.Modules.GlobalUtilities.DBCompositeTypes;

namespace RaaiVan.Modules.CoreNetwork
{
    public static class CNController
    {
        public static string GetFullyQualifiedName(string name)
        {
            return "[dbo]." + "[CN_" + name + "]"; //'[dbo].' is database owner and 'CN_' is module qualifier
        }

        public static bool initialize(Guid applicationId)
        {
            return DBConnector.succeed(applicationId, GetFullyQualifiedName("Initialize"));
        }

        public static bool add_node_type(Guid applicationId, NodeType Info, Guid? templateFormId = null)
        {
            if (Info.AdditionalID.HasValue) Info.NodeTypeAdditionalID = Info.AdditionalID.Value.ToString();

            return DBConnector.succeed(applicationId, GetFullyQualifiedName("AddNodeType"),
                applicationId, Info.NodeTypeID, Info.NodeTypeAdditionalID, Info.Name, Info.ParentID,
                Info.IsService, Info.TemplateTypeID, templateFormId, Info.CreatorUserID, DateTime.Now);
        }

        public static bool rename_node_type(Guid applicationId, NodeType Info)
        {
            return DBConnector.succeed(applicationId, GetFullyQualifiedName("RenameNodeType"),
                applicationId, Info.NodeTypeID, Info.Name, Info.LastModifierUserID, Info.LastModificationDate);
        }

        public static bool set_node_type_additional_id(Guid applicationId, Guid nodeTypeId,
            string additionalId, Guid currentUserId, ref string errorMessage)
        {
            return DataProvider.SetNodeTypeAdditionalID(applicationId, nodeTypeId, 
                additionalId, currentUserId, ref errorMessage);
        }

        public static bool set_additional_id_pattern(Guid applicationId, NodeType Info)
        {
            return DataProvider.SetAdditionalIDPattern(applicationId, Info);
        }
        
        public static bool move_node_type(Guid applicationId, 
            List<Guid> nodeTypeIds, Guid? parentId, Guid currentUserId)
        {
            return DataProvider.MoveNodeType(applicationId, nodeTypeIds, parentId, currentUserId);
        }
        
        public static List<NodeType> get_node_types(Guid applicationId, List<Guid> nodeTypeIds, bool grabSubNodeTypes = false)
        {
            List<NodeType> retList = new List<NodeType>();
            DataProvider.GetNodeTypes(applicationId, ref retList, nodeTypeIds, grabSubNodeTypes);
            return retList;
        }

        public static List<NodeType> get_node_types(Guid applicationId, string searchText, bool? isKnowledge,
            bool? isDocument, bool? archive, List<ExtensionType> extensions, int? count, long? lowerBoundary, ref long totalCount)
        {
            List<NodeType> retList = new List<NodeType>();
            DataProvider.GetNodeTypes(applicationId, ref retList, searchText, isKnowledge,
                isDocument, archive, extensions, count, lowerBoundary, ref totalCount);
            return retList;
        }

        public static List<NodeType> get_node_types(Guid applicationId, 
            string searchText, List<ExtensionType> extensions, bool? archive = false)
        {
            long totalCount = 0;
            return CNController.get_node_types(applicationId, searchText, null, null, archive, extensions, null, null, ref totalCount);
        }

        public static NodeType get_node_type_by_node_id(Guid applicationId, Guid nodeId)
        {
            return DataProvider.GetNodeType(applicationId, null, null, nodeId);
        }

        public static NodeType get_node_type(Guid applicationId, Guid nodeTypeId)
        {
            return DataProvider.GetNodeType(applicationId, nodeTypeId, null, null);
        }

        public static NodeType get_node_type(Guid applicationId, NodeTypes nodeType)
        {
            return DataProvider.GetNodeType(applicationId, null, nodeType, null);
        }

        public static List<Guid> have_child_node_types(Guid applicationId, ref List<Guid> nodeTypeIds)
        {
            List<Guid> retList = new List<Guid>();
            DataProvider.HaveChildNodeTypes(applicationId, ref retList, ref nodeTypeIds);
            return retList;
        }

        public static List<Guid> have_child_node_types(Guid applicationId, List<Guid> nodeTypeIds)
        {
            return have_child_node_types(applicationId, ref nodeTypeIds);
        }

        public static List<NodeType> get_child_node_types(Guid applicationId, Guid? parentId, bool? archive = false)
        {
            List<NodeType> retList = new List<NodeType>();
            DataProvider.GetChildNodeTypes(applicationId, ref retList, parentId, archive);
            return retList;
        }

        public static bool remove_node_types(Guid applicationId, 
            List<Guid> nodeTypeIds, bool? removeHierarchy, Guid currentUserId)
        {
            return DataProvider.ArithmeticDeleteNodeTypes(applicationId, ref nodeTypeIds, removeHierarchy, currentUserId);
        }

        public static bool remove_node_type(Guid applicationId, 
            Guid nodeTypeId, bool? removeHierarchy, Guid currentUserId)
        {
            return remove_node_types(applicationId, new List<Guid>() { nodeTypeId }, removeHierarchy, currentUserId);
        }

        public static bool recover_node_type(Guid applicationId, NodeType Info)
        {
            return DataProvider.RecoverNodeType(applicationId, Info);
        }

        public static bool add_relation_type(Guid applicationId, RelationType Info)
        {
            return DataProvider.AddRelationType(applicationId, Info);
        }

        public static bool modify_relation_type(Guid applicationId, RelationType Info)
        {
            return DataProvider.ModifyRelationType(applicationId, Info);
        }

        public static List<RelationType> get_relation_types(Guid applicationId)
        {
            List<RelationType> retList = new List<RelationType>();
            DataProvider.GetRelationTypes(applicationId, ref retList);
            return retList;
        }

        public static bool remove_relation_type(Guid applicationId, RelationType Info)
        {
            return DataProvider.ArithmeticDeleteRelationType(applicationId, Info);
        }

        public static bool add_relations(Guid applicationId, ref List<Relation> relations, Guid creatorUserId)
        {
            return DataProvider.AddRelations(applicationId, ref relations, creatorUserId);
        }

        public static bool add_relation(Guid applicationId, Relation relation, Guid creatorUserId)
        {
            List<Relation> _lst = new List<Relation>();
            _lst.Add(relation);
            return add_relations(applicationId, ref _lst, creatorUserId);
        }

        public static bool save_relations(Guid applicationId, 
            Guid nodeId, List<Guid> relatedNodeIds, Guid currentUserId)
        {
            return DataProvider.SaveRelations(applicationId, nodeId, relatedNodeIds, currentUserId);
        }

        public static bool make_parent(Guid applicationId, ref List<Relation> relations, Guid creatorUserId)
        {
            return DataProvider.MakeParent(applicationId, ref relations, creatorUserId);
        }

        public static bool make_parent(Guid applicationId, Relation relation, Guid creatorUserId)
        {
            List<Relation> _lst = new List<Relation>();
            _lst.Add(relation);
            return make_parent(applicationId, ref _lst, creatorUserId);
        }

        public static bool make_correlations(Guid applicationId, ref List<Relation> relations, Guid creatorUserId)
        {
            return DataProvider.MakeCorrelation(applicationId, ref relations, creatorUserId);
        }

        public static bool make_correlation(Guid applicationId, Relation relation, Guid creatorUserId)
        {
            List<Relation> _lst = new List<Relation>();
            _lst.Add(relation);
            return make_correlations(applicationId, ref _lst, creatorUserId);
        }

        public static bool remove_relations(Guid applicationId, 
            ref List<Relation> relations, Guid? relationTypeId, Guid lastModifierUserId, bool? reverseAlso = null)
        {
            return DataProvider.ArithmeticDeleteRelations(applicationId,
                ref relations, relationTypeId, lastModifierUserId, reverseAlso);
        }

        public static bool remove_relation(Guid applicationId, 
            Relation relation, Guid? relationTypeId, Guid lastModifierUserId, bool? reverseAlso = null)
        {
            List<Relation> _lst = new List<Relation>();
            _lst.Add(relation);
            return remove_relations(applicationId, ref _lst, relationTypeId, lastModifierUserId, reverseAlso);
        }

        public static bool remove_correlation(Guid applicationId, ref List<Relation> relations, Guid lastModifierUserId)
        {
            return DataProvider.ArithmeticDeleteCorrelation(applicationId, ref relations, lastModifierUserId);
        }

        public static bool remove_correlation(Guid applicationId, Relation relation, Guid lastModifierUserId)
        {
            List<Relation> _lst = new List<Relation>();
            _lst.Add(relation);
            return remove_correlation(applicationId, ref _lst, lastModifierUserId);
        }

        public static bool unparent(Guid applicationId, ref List<Relation> relations, Guid lastModifierUserId)
        {
            return DataProvider.Unparent(applicationId, ref relations, lastModifierUserId);
        }

        public static bool unparent(Guid applicationId, Relation relation, Guid lastModifierUserId)
        {
            List<Relation> _lst = new List<Relation>();
            _lst.Add(relation);
            return unparent(applicationId, ref _lst, lastModifierUserId);
        }

        public static bool add_node(Guid applicationId, Node Info, bool? addMember = null)
        {
            return DataProvider.AddNode(applicationId, Info, null, null, addMember);
        }

        public static bool add_node(Guid applicationId, Node Info, NodeTypes nodeType, bool? addMember = null)
        {
            return DataProvider.AddNode(applicationId, Info, nodeType, null, addMember);
        }

        public static bool add_node(Guid applicationId, Node Info, string nodeTypeAdditionalId)
        {
            return DataProvider.AddNode(applicationId, Info, null, nodeTypeAdditionalId, null);
        }

        public static bool set_additional_id(Guid applicationId,
            Guid id, string additionalId_main, string additionalId, Guid currentUserId)
        {
            return DataProvider.SetAdditionalID(applicationId, id, additionalId_main, additionalId, currentUserId);
        }

        public static bool modify_node(Guid applicationId, Node Info)
        {
            return DataProvider.ModifyNode(applicationId, Info);
        }

        public static bool change_node_type(Guid applicationId, Guid nodeId, Guid nodeTypeId, Guid currentUserId)
        {
            return DataProvider.ChangeNodeType(applicationId, nodeId, nodeTypeId, currentUserId);
        }

        public static bool set_document_tree_node_id(Guid applicationId, 
            List<Guid> nodeIds, Guid? documentTreeNodeId, Guid currentUserId)
        {
            return DataProvider.SetDocumentTreeNodeID(applicationId, nodeIds, documentTreeNodeId, currentUserId);
        }

        public static bool set_document_tree_node_id(Guid applicationId, 
            Guid nodeId, Guid? documentTreeNodeId, Guid currentUserId)
        {
            List<Guid> lst = new List<Guid>();
            lst.Add(nodeId);
            return CNController.set_document_tree_node_id(applicationId, lst, documentTreeNodeId, currentUserId);
        }

        public static bool modify_node_name(Guid applicationId, Node Info)
        {
            return DataProvider.ModifyNodeName(applicationId, Info);
        }

        public static bool modify_node_description(Guid applicationId, Node Info)
        {
            return DataProvider.ModifyNodeDescription(applicationId, Info);
        }

        public static bool modify_node_public_description(Guid applicationId, Guid nodeId, string description)
        {
            return DataProvider.ModifyNodePublicDescription(applicationId, nodeId, description);
        }

        public static bool set_node_expiration_date(Guid applicationId, Guid nodeId, DateTime? expirationDate)
        {
            return DataProvider.SetNodeExpirationDate(applicationId, nodeId, expirationDate);
        }

        public static bool set_expired_nodes_as_not_searchable(Guid applicationId)
        {
            return DataProvider.SetExpiredNodesAsNotSearchable(applicationId);
        }

        public static List<Guid> get_node_ids_that_will_be_expired_soon(Guid applicationId)
        {
            return DataProvider.GetNodeIDsThatWillBeExpiredSoon(applicationId);
        }

        public static bool NotifyNodeExpiration(Guid applicationId, Guid nodeId, Guid userId,
            ref List<Dashboard> retDashboards)
        {
            return DataProvider.NotifyNodeExpiration(applicationId, nodeId, userId, ref retDashboards);
        }

        public static bool set_previous_version(Guid applicationId, 
            Guid nodeId, Guid? previousVersionId, Guid currentUserId)
        {
            return DataProvider.SetPreviousVersion(applicationId, nodeId, previousVersionId, currentUserId);
        }

        public static List<Node> get_previous_versions(Guid applicationId, 
            Guid nodeId, Guid? currentUserId = null, bool? checkPrivacy = false)
        {
            List<Node> retList = new List<Node>();
            DataProvider.GetPreviousVersions(applicationId, ref retList, nodeId, currentUserId, checkPrivacy);
            return retList;
        }

        public static List<Node> get_new_versions(Guid applicationId, Guid nodeId)
        {
            List<Node> retList = new List<Node>();
            DataProvider.GetNewVersions(applicationId, ref retList, nodeId);
            return retList;
        }

        public static bool modify_node_tags(Guid applicationId, Node Info)
        {
            return DataProvider.ModifyNodeTags(applicationId, Info);
        }

        public static string get_node_description(Guid applicationId, Guid nodeId)
        {
            return DataProvider.GetNodeDescription(applicationId, nodeId);
        }

        public static bool set_nodes_searchability(Guid applicationId, 
            List<Guid> nodeIds, bool searchable, Guid currentUserId)
        {
            return DataProvider.SetNodesSearchability(applicationId, ref nodeIds, searchable, currentUserId);
        }

        public static bool set_node_searchability(Guid applicationId, Guid nodeId, bool searchable, Guid currentUserId)
        {
            List<Guid> lst = new List<Guid>();
            lst.Add(nodeId);
            return CNController.set_nodes_searchability(applicationId, lst, searchable, currentUserId);
        }

        public static bool remove_nodes(Guid applicationId, 
            ref List<Guid> nodeIds, bool? removeHierarchy, Guid lastModifierUserId)
        {
            return DataProvider.ArithmeticDeleteNodes(applicationId, ref nodeIds, removeHierarchy, lastModifierUserId);
        }

        public static bool remove_node(Guid applicationId, Guid nodeId, bool? removeHierarchy, Guid lastModifierUserId)
        {
            List<Guid> _nIds = new List<Guid>();
            _nIds.Add(nodeId);
            return remove_nodes(applicationId, ref _nIds, removeHierarchy, lastModifierUserId);
        }

        public static bool recycle_nodes(Guid applicationId, List<Guid> nodeIds, Guid currentUserId)
        {
            return DataProvider.RecycleNodes(applicationId, ref nodeIds, currentUserId);
        }

        public static bool set_node_types_order(Guid applicationId, List<Guid> nodeTypeIds)
        {
            return DataProvider.SetNodeTypesOrder(applicationId, nodeTypeIds);
        }

        public static bool set_nodes_order(Guid applicationId, List<Guid> nodeIds)
        {
            return DataProvider.SetNodesOrder(applicationId, nodeIds);
        }

        private static List<NodesCount> _get_nodes_count(Guid applicationId, 
            ref List<Guid> nodeTypeIds, NodeTypes? nodeType, DateTime? lowerCreationDateLimit = null, 
            DateTime? upperCreationDateLimit = null, bool? archive = false, bool? root = null)
        {
            List<NodesCount> retList = new List<NodesCount>();
            DataProvider.GetNodesCount(applicationId, ref retList, ref nodeTypeIds, nodeType, 
                lowerCreationDateLimit, upperCreationDateLimit, root, archive);
            return retList;
        }

        public static List<NodesCount> get_nodes_count(Guid applicationId, DateTime? lowerCreationDateLimit = null,
            DateTime? upperCreationDateLimit = null, bool? archive = false, bool? root = null)
        {
            List<Guid> lst = new List<Guid>();
            return _get_nodes_count(applicationId, ref lst, null, lowerCreationDateLimit, 
                upperCreationDateLimit, archive, root);
        }

        public static NodesCount get_nodes_count(Guid applicationId, Guid nodeTypeId, 
            DateTime? lowerCreationDateLimit = null, DateTime? upperCreationDateLimit = null, 
            bool? archive = false, bool? root = null)
        {
            List<Guid> lst = new List<Guid>();
            lst.Add(nodeTypeId);
            return _get_nodes_count(applicationId, ref lst, null, lowerCreationDateLimit, upperCreationDateLimit, 
                archive, root).FirstOrDefault();
        }

        public static List<NodesCount> get_nodes_count(Guid applicationId, ref List<Guid> nodeTypeIds, 
            DateTime? lowerCreationDateLimit = null, DateTime? upperCreationDateLimit = null, 
            bool? archive = false, bool? root = null)
        {
            return _get_nodes_count(applicationId, ref nodeTypeIds, null, 
                lowerCreationDateLimit, upperCreationDateLimit, archive, root);
        }

        public static NodesCount get_nodes_count(Guid applicationId, NodeTypes nodeType, 
            DateTime? lowerCreationDateLimit = null, DateTime? upperCreationDateLimit = null, 
            bool? archive = false, bool? root = null)
        {
            List<Guid> lst = new List<Guid>();
            return _get_nodes_count(applicationId, ref lst, nodeType, lowerCreationDateLimit, 
                upperCreationDateLimit, archive, root).FirstOrDefault();
        }

        public static List<NodesCount> get_most_populated_node_types(Guid applicationId, 
            int? count = null, int? lowerBoundary = null)
        {
            List<NodesCount> lst = new List<NodesCount>();
            DataProvider.GetMostPopulatedNodeTypes(applicationId, ref lst, count, lowerBoundary);
            return lst;
        }

        public static int get_node_records_count(Guid applicationId)
        {
            return DataProvider.GetNodeRecordsCount(applicationId);
        }

        public static List<Guid> get_node_type_ids(Guid applicationId, List<string> nodeTypeAdditionalIds)
        {
            return DataProvider.GetNodeTypeIDs(applicationId, nodeTypeAdditionalIds);
        }

        public static Guid? get_node_type_id(Guid applicationId, string nodeTypeAdditionalId)
        {
            List<Guid> ids = get_node_type_ids(applicationId, new List<string>() { nodeTypeAdditionalId });

            if (ids.Count == 0) return null;
            else return ids[0];
        }

        private static List<Guid> _get_node_ids(Guid applicationId, List<string> nodeAdditionalIds, 
            Guid? nodeTypeId, NodeTypes? nodeType, string strNodeTypeAdditionalId)
        {
            List<Guid> retList = new List<Guid>();
            DataProvider.GetNodeIDs(applicationId, ref retList, nodeAdditionalIds, nodeTypeId, nodeType, strNodeTypeAdditionalId);
            return retList;
        }

        public static List<Guid> get_node_ids(Guid applicationId, List<string> nodeAdditionalIds, Guid nodeTypeId)
        {
            return _get_node_ids(applicationId, nodeAdditionalIds, nodeTypeId, null, null);
        }

        public static List<Guid> get_node_ids(Guid applicationId, List<string> nodeAdditionalIds, NodeTypes nodeType)
        {
            return _get_node_ids(applicationId, nodeAdditionalIds, null, nodeType, null);
        }

        public static List<Guid> get_node_ids(Guid applicationId, List<string> nodeAdditionalIds, string nodeTypeAdditionalId)
        {
            return _get_node_ids(applicationId, nodeAdditionalIds, null, null, nodeTypeAdditionalId);
        }

        public static Guid get_node_id(Guid applicationId, string nodeAdditionalId, Guid nodeTypeId)
        {
            List<string> _aIds = new List<string>();
            _aIds.Add(nodeAdditionalId);
            return _get_node_ids(applicationId, _aIds, nodeTypeId, null, null).FirstOrDefault();
        }

        public static Guid get_node_id(Guid applicationId, string nodeAdditionalId, NodeTypes nodeType)
        {
            List<string> _aIds = new List<string>();
            _aIds.Add(nodeAdditionalId);
            return _get_node_ids(applicationId, _aIds, null, nodeType, null).FirstOrDefault();
        }

        public static Guid get_node_id(Guid applicationId, string nodeAdditionalId, string nodeTypeAdditionalId)
        {
            List<string> _aIds = new List<string>();
            _aIds.Add(nodeAdditionalId);
            return _get_node_ids(applicationId, _aIds, null, null, nodeTypeAdditionalId).FirstOrDefault();
        }

        public static List<Guid> get_node_ids(Guid applicationId, ref List<Node> nodes)
        {
            List<Guid> retList = new List<Guid>();
            DataProvider.GetNodeIDs(applicationId, ref retList, ref nodes);
            return retList;
        }

        public static List<Guid> get_node_ids(Guid applicationId, List<Node> nodes)
        {
            return get_node_ids(applicationId, ref nodes);
        }

        public static Dictionary<string, Guid> get_node_ids(Guid applicationId, Guid nodeTypeId, List<string> additionalIds)
        {
            return DataProvider.GetNodeIDs(applicationId, nodeTypeId, additionalIds);
        }

        public static List<Node> get_nodes(Guid applicationId, 
            List<Guid> nodeIds, bool? full = null, Guid? currentUserId = null)
        {
            List<Node> retList = new List<Node>();
            DataProvider.GetNodes(applicationId, ref retList, ref nodeIds, full, currentUserId);
            return retList;
        }

        public static Node get_node(Guid applicationId, 
            Guid nodeId, bool? full = null, Guid? currentUserId = null)
        {
            List<Guid> _nIds = new List<Guid>();
            _nIds.Add(nodeId);
            return get_nodes(applicationId, _nIds, full, currentUserId).FirstOrDefault();
        }

        private static List<Node> get_nodes(Guid applicationId,
            ref long totalCount, ref List<NodesCount> nodesCount, ref Dictionary<string, object> groupedResults,
            List<Guid> nodeTypeIds = null, NodeTypes? nodeType = null, bool? useNodeTypeHierarchy = null, 
            Guid? relatedToNodeId = null, string searchText = null, bool? isDocument = null, bool? isKnowledge = null, 
            DateTime? lowerCreationDateLimit = null, DateTime? upperCreationDateLimit = null,
            int? count = null, long? lowerBoundary = null, bool? searchable = null, bool? archive = null, 
            bool? grabNoContentServices = null, List<FormFilter> filters = null, bool? matchAllFilters = null, 
            bool? fetchCounts = null, Guid? currentUserId = null, Guid? creatorUserId = null,
            bool checkAccess = false, Guid? groupByFormElementId = null)
        {
            //prepare
            if (filters == null) filters = new List<FormFilter>();

            string strAddId = null;
            if (nodeType != null) strAddId = CNUtilities.get_node_type_additional_id(nodeType.Value).ToString();
            //end of prepare

            DBCompositeType<FormFilterTableType> formFilters = new DBCompositeType<FormFilterTableType>()
                .add(filters.Select(f => new FormFilterTableType(
                    elementId: f.ElementID,
                    ownerId: f.OwnerID,
                    text: f.Text,
                    textItems: ProviderUtil.list_to_string<string>(f.TextItems),
                    or: f.Or,
                    exact: f.Exact,
                    dateFrom: f.DateFrom,
                    dateTo: f.DateTo,
                    floatFrom: f.FloatFrom,
                    floatTo: f.FloatTo,
                    bit: f.Bit,
                    guid: f.Guid,
                    guidItems: ProviderUtil.list_to_string<Guid>(f.GuidItems),
                    compulsory: f.Compulsory)).ToList());

            DBResultSet results = DBConnector.read(applicationId, DataProvider.GetFullyQualifiedName("GetNodes"),
                applicationId,
                currentUserId,
                nodeTypeIds == null || nodeTypeIds.Count == 0 ? null : ProviderUtil.list_to_string<Guid>(nodeTypeIds),
                ',',
                string.IsNullOrEmpty(strAddId) ? null : strAddId,
                useNodeTypeHierarchy,
                relatedToNodeId,
                string.IsNullOrEmpty(searchText) ? null : ProviderUtil.get_search_text(searchText),
                isDocument,
                isKnowledge,
                creatorUserId,
                searchable,
                archive,
                grabNoContentServices,
                lowerCreationDateLimit,
                upperCreationDateLimit,
                count,
                lowerBoundary,
                formFilters,
                matchAllFilters,
                fetchCounts,
                checkAccess,
                RaaiVanSettings.DefaultPrivacy(applicationId),
                groupByFormElementId);

            if (groupByFormElementId.HasValue && groupByFormElementId != Guid.Empty)
            {
                groupedResults = CNParsers.node_counts_grouped_by_element(results);
                return new List<Node>();
            }
            else
            {
                return CNParsers.nodes(results, ref nodesCount, ref totalCount, full: false,
                    fetchCounts: fetchCounts.HasValue && fetchCounts.Value);
            }
        }

        public static List<Node> get_nodes(Guid applicationId,
            List<Guid> nodeTypeIds = null, NodeTypes? nodeType = null, bool? useNodeTypeHierarchy = null,
            Guid? relatedToNodeId = null, string searchText = null, bool? isDocument = null, bool? isKnowledge = null,
            DateTime? lowerCreationDateLimit = null, DateTime? upperCreationDateLimit = null,
            int? count = null, long? lowerBoundary = null, bool? searchable = null, bool? archive = null,
            bool? grabNoContentServices = null, List<FormFilter> filters = null, bool? matchAllFilters = null,
            Guid? currentUserId = null, Guid? creatorUserId = null, bool checkAccess = false)
        {
            long totalCount = 0;
            List<NodesCount> retNodesCount = new List<NodesCount>();
            Dictionary<string, object> groupedResults = new Dictionary<string, object>();
            bool? fetchCounts = null;
            Guid? groupByFormElementId = null;

            return get_nodes(applicationId, ref totalCount, ref retNodesCount, ref groupedResults, nodeTypeIds,
                nodeType, useNodeTypeHierarchy, relatedToNodeId, searchText, isDocument, isKnowledge,
                lowerCreationDateLimit, upperCreationDateLimit, count, lowerBoundary, searchable, archive,
                grabNoContentServices, filters, matchAllFilters, fetchCounts, currentUserId, creatorUserId,
                checkAccess, groupByFormElementId);
        }

        public static List<Node> get_nodes(Guid applicationId, ref long totalCount, ref List<NodesCount> nodesCount,
            List<Guid> nodeTypeIds = null, NodeTypes? nodeType = null, bool? useNodeTypeHierarchy = null,
            Guid? relatedToNodeId = null, string searchText = null, bool? isDocument = null, bool? isKnowledge = null,
            DateTime? lowerCreationDateLimit = null, DateTime? upperCreationDateLimit = null,
            int? count = null, long? lowerBoundary = null, bool? searchable = null, bool? archive = null,
            bool? grabNoContentServices = null, List<FormFilter> filters = null, bool? matchAllFilters = null,
            bool? fetchCounts = null, Guid ? currentUserId = null, Guid? creatorUserId = null, bool checkAccess = false)
        {
            Dictionary<string, object> groupedResults = new Dictionary<string, object>();
            Guid? groupByFormElementId = null;

            return get_nodes(applicationId, ref totalCount, ref nodesCount, ref groupedResults, nodeTypeIds,
                nodeType, useNodeTypeHierarchy, relatedToNodeId, searchText, isDocument, isKnowledge,
                lowerCreationDateLimit, upperCreationDateLimit, count, lowerBoundary, searchable, archive,
                grabNoContentServices, filters, matchAllFilters, fetchCounts, currentUserId, creatorUserId,
                checkAccess, groupByFormElementId);
        }

        public static List<Node> get_nodes(Guid applicationId, ref long totalCount,
            List<Guid> nodeTypeIds = null, NodeTypes? nodeType = null, bool? useNodeTypeHierarchy = null,
            Guid? relatedToNodeId = null, string searchText = null, bool? isDocument = null, bool? isKnowledge = null,
            DateTime? lowerCreationDateLimit = null, DateTime? upperCreationDateLimit = null,
            int? count = null, long? lowerBoundary = null, bool? searchable = null, bool? archive = null,
            bool? grabNoContentServices = null, List<FormFilter> filters = null, bool? matchAllFilters = null,
            Guid? currentUserId = null, Guid? creatorUserId = null, bool checkAccess = false)
        {
            List<NodesCount> nodesCount = new List<NodesCount>();
            bool? fetchCounts = null;
            Dictionary<string, object> groupedResults = new Dictionary<string, object>();
            Guid? groupByFormElementId = null;

            return get_nodes(applicationId, ref totalCount, ref nodesCount, ref groupedResults, nodeTypeIds,
                nodeType, useNodeTypeHierarchy, relatedToNodeId, searchText, isDocument, isKnowledge,
                lowerCreationDateLimit, upperCreationDateLimit, count, lowerBoundary, searchable, archive,
                grabNoContentServices, filters, matchAllFilters, fetchCounts, currentUserId, creatorUserId,
                checkAccess, groupByFormElementId);
        }

        public static Dictionary<string, object> get_nodes_grouped(Guid applicationId, Guid nodeTypeId, 
            Guid groupByFormElementId, Guid? relatedToNodeId = null, string searchText = null, bool? isDocument = null, 
            bool? isKnowledge = null, DateTime? lowerCreationDateLimit = null, DateTime? upperCreationDateLimit = null, 
            bool? searchable = null, List<FormFilter> filters = null, bool? matchAllFilters = null,
            Guid? currentUserId = null, Guid? creatorUserId = null, bool checkAccess = false)
        {
            long totalCount = 0;
            List<NodesCount> nodesCount = new List<NodesCount>();
            Dictionary<string, object> groupedResults = new Dictionary<string, object>();

            get_nodes(applicationId: applicationId, totalCount: ref totalCount, nodesCount: ref nodesCount, 
                groupedResults: ref groupedResults, nodeTypeIds: new List<Guid>() { nodeTypeId },
                relatedToNodeId: relatedToNodeId, searchText: searchText, isDocument: isDocument, isKnowledge: isKnowledge,
                lowerCreationDateLimit: lowerCreationDateLimit, upperCreationDateLimit: upperCreationDateLimit, 
                searchable: searchable, filters: filters, matchAllFilters: matchAllFilters, currentUserId: currentUserId, 
                creatorUserId: creatorUserId, checkAccess: checkAccess, groupByFormElementId: groupByFormElementId);

            return groupedResults == null ? new Dictionary<string, object>() : groupedResults;
        }

        public static List<Node> get_most_popular_nodes(Guid applicationId, List<Guid> nodeTypeIds, Guid? parentNodeId,
            int count, long? lowerBoundary, ref long totalCount, bool? archive = false, bool? searchable = true)
        {
            List<Node> retList = new List<Node>();
            DataProvider.GetMostPopularNodes(applicationId, ref retList, nodeTypeIds, parentNodeId, count, 
                lowerBoundary, ref totalCount);
            return retList;
        }

        public static List<Node> get_most_popular_nodes(Guid applicationId, Guid? nodeTypeId, Guid? parentNodeId, 
            int count, long? lowerBoundary, ref long totalCount)
        {
            List<Guid> nodeTypeIds = new List<Guid>();
            if (nodeTypeId.HasValue && nodeTypeId != Guid.Empty) nodeTypeIds.Add(nodeTypeId.Value);
            return get_most_popular_nodes(applicationId, nodeTypeIds, parentNodeId, count, lowerBoundary, ref totalCount);
        }

        public static List<Node> get_parent_nodes(Guid applicationId, Guid nodeId)
        {
            List<Node> retList = new List<Node>();
            DataProvider.GetParentNodes(applicationId, ref retList, nodeId);
            return retList;
        }

        public static List<Node> get_child_nodes(Guid applicationId, Guid nodeId)
        {
            List<Node> retList = new List<Node>();
            DataProvider.GetChildNodes(applicationId, ref retList, nodeId);
            return retList;
        }

        public static List<Node> get_default_related_nodes(Guid applicationId, Guid nodeId)
        {
            List<Node> retList = new List<Node>();
            DataProvider.GetDefaultRelatedNodes(applicationId, ref retList, nodeId);
            return retList;
        }

        public static List<Node> get_default_connected_nodes(Guid applicationId, Guid nodeId)
        {
            List<Node> retList = new List<Node>();
            DataProvider.GetDefaultConnectedNodes(applicationId, ref retList, nodeId);
            return retList;
        }

        public static List<Node> get_brother_nodes(Guid applicationId, Guid nodeId)
        {
            List<Node> retList = new List<Node>();
            DataProvider.GetBrotherNodes(applicationId, ref retList, nodeId);
            return retList;
        }
        
        public static List<Node> get_direct_childs(Guid applicationId, Guid? nodeId, Guid? nodeTypeId, 
            string nodeTypeAdditionalId, bool? searchable, double? lowerBoundary, int? count, 
            string orderBy, bool? orderByDesc, string searchText, bool? checkAccess, Guid? currentUserId, ref long totalCount)
        {
            List<Node> retList = new List<Node>();
            DataProvider.GetDirectChilds(applicationId, ref retList, nodeId, nodeTypeId, nodeTypeAdditionalId, searchable,
                lowerBoundary, count, orderBy, orderByDesc, searchText, checkAccess, currentUserId, ref totalCount);
            return retList;
        }

        public static List<Node> get_direct_childs(Guid applicationId, Guid nodeId)
        {
            long totalCount = 0;
            return get_direct_childs(applicationId, nodeId, null, null, true, null, null, null, null, null, null, null, ref totalCount);
        }

        public static Node get_direct_parent(Guid applicationId, Guid nodeId)
        {
            return DataProvider.GetDirectParent(applicationId, nodeId);
        }

        public static bool set_direct_parent(Guid applicationId, 
            ref List<Guid> nodeIds, Guid? parentNodeId, Guid currentUserId, ref string errorMessage)
        {
            return DataProvider.SetDirectParent(applicationId, ref nodeIds, parentNodeId, currentUserId, ref errorMessage);
        }

        public static bool set_direct_parent(Guid applicationId, List<Guid> nodeIds, Guid? parentNodeId, 
            Guid currentUserId, ref string errorMessage)
        {
            return set_direct_parent(applicationId, ref nodeIds, parentNodeId, currentUserId, ref errorMessage);
        }

        public static bool set_direct_parent(Guid applicationId, Guid nodeId, Guid? parentNodeId, 
            Guid currentUserId, ref string errorMessage)
        {
            List<Guid> _nIds = new List<Guid>();
            _nIds.Add(nodeId);
            return set_direct_parent(applicationId, ref _nIds, parentNodeId, currentUserId, ref errorMessage);
        }

        public static List<Guid> have_childs(Guid applicationId, ref List<Guid> nodeIds)
        {
            List<Guid> retList = new List<Guid>();
            DataProvider.HaveChilds(applicationId, ref retList, ref nodeIds);
            return retList;
        }

        public static List<Guid> have_childs(Guid applicationId, List<Guid> nodeIds)
        {
            return have_childs(applicationId, ref nodeIds);
        }

        public static bool has_childs(Guid applicationId, Guid nodeId)
        {
            List<Guid> _nIds = new List<Guid>();
            _nIds.Add(nodeId);
            _nIds = have_childs(applicationId, ref _nIds);
            return _nIds != null && _nIds.Count > 0;
        }

        public static List<Guid> get_related_node_ids(Guid applicationId, Guid nodeId, Guid? nodeTypeId, 
            string searchText, bool? inRelations, bool? outRelations,
            bool? inTagRelations = null, bool? outTagRelations = null, int? count = null, int? lowerBoundary = null)
        {
            List<Guid> retList = new List<Guid>();
            DataProvider.GetRelatedNodeIDs(applicationId, ref retList, nodeId, nodeTypeId, 
                searchText, inRelations, outRelations, inTagRelations, outTagRelations, count, lowerBoundary);
            return retList;
        }
        
        public static List<Node> get_related_nodes(Guid applicationId, Guid nodeId, Guid? nodeTypeId, 
            string searchText, bool? inRelations, bool? outRelations,
            bool? inTagRelations = null, bool? outTagRelations = null, int? count = null, int? lowerBoundary = null)
        {
            List<Node> retList = new List<Node>();
            DataProvider.GetRelatedNodes(applicationId, ref retList, nodeId, nodeTypeId,
                searchText, inRelations, outRelations, inTagRelations, outTagRelations, count, lowerBoundary);
            return retList;
        }

        public static List<NodesCount> get_related_nodes_count(Guid applicationId, Guid nodeId, Guid? nodeTypeId,
            string searchText, bool? inRelations, bool? outRelations,
            bool? inTagRelations = null, bool? outTagRelations = null, int? count = null, int? lowerBoundary = null)
        {
            List<NodesCount> retList = new List<NodesCount>();
            DataProvider.GetRelatedNodesCount(applicationId, ref retList, nodeId, nodeTypeId,
                searchText, inRelations, outRelations, inTagRelations, outTagRelations, count, lowerBoundary);
            return retList;
        }

        public static List<Node> get_related_nodes_partitioned(Guid applicationId, Guid nodeId, List<Guid> nodeTypeIds,
            bool? inRelations, bool? outRelations, bool? inTagRelations = null, bool? outTagRelations = null, int? count = null)
        {
            List<Node> retList = new List<Node>();
            DataProvider.GetRelatedNodesPartitioned(applicationId, ref retList, nodeId, nodeTypeIds,
                inRelations, outRelations, inTagRelations, outTagRelations, count);
            return retList;
        }

        public static bool relation_exists(Guid applicationId, 
            Guid sourceNodeId, Guid destinationNodeId, bool? reverseAlso = null)
        {
            return DataProvider.RelationExists(applicationId, sourceNodeId, destinationNodeId, reverseAlso);
        }
        
        public static bool add_member(Guid applicationId, NodeMember Info, ref List<Dashboard> retDashboards)
        {
            return DataProvider.AddMember(applicationId, Info, ref retDashboards);
        }

        public static bool remove_member(Guid applicationId, Guid nodeId, Guid userId)
        {
            return DataProvider.ArithmeticDeleteMember(applicationId, nodeId, userId);
        }

        public static bool accept_member(Guid applicationId, Guid nodeId, Guid userId)
        {
            return DataProvider.AcceptMember(applicationId, nodeId, userId);
        }

        public static bool set_member_position(Guid applicationId, Guid nodeId, Guid userId, string position)
        {
            return DataProvider.SetMemberPosition(applicationId, nodeId, userId, position);
        }

        public static bool is_node_creator(Guid applicationId, 
            Guid? nodeId, Guid? nodeTypeId, string additionalId, Guid userId)
        {
            return DataProvider.IsNodeCreator(applicationId, nodeId, nodeTypeId, additionalId, userId);
        }

        public static bool is_node_creator(Guid applicationId, Guid nodeId, Guid userId)
        {
            return is_node_creator(applicationId, nodeId, null, null, userId);
        }

        public static bool is_node_creator(Guid applicationId, Guid? nodeTypeId, string additionalId, Guid userId)
        {
            return is_node_creator(applicationId, null, nodeTypeId, additionalId, userId);
        }

        public static bool is_node_member(Guid applicationId, 
            Guid nodeId, Guid userId, bool? isAdmin = null, NodeMemberStatuses? status = null)
        {
            return DataProvider.IsNodeMember(applicationId, nodeId, userId, isAdmin, status);
        }

        public static bool has_admin(Guid applicationId, Guid nodeId)
        {
            return DataProvider.HasAdmin(applicationId, nodeId);
        }

        public static bool set_unset_node_admin(Guid applicationId, 
            Guid nodeId, Guid userId, bool admin, bool unique = false)
        {
            return DataProvider.SetUnsetNodeAdmin(applicationId, nodeId, userId, admin, unique);
        }

        public static bool is_admin_member(Guid applicationId, Guid nodeId, Guid userId)
        {
            return DataProvider.IsNodeAdmin(applicationId, nodeId, userId);
        }

        public static List<Guid> get_complex_admins(Guid applicationId, Guid listIdOrNodeId)
        {
            List<Guid> retList = new List<Guid>();
            DataProvider.GetComplexAdmins(applicationId, ref retList, listIdOrNodeId);
            return retList;
        }

        public static bool is_complex_admin(Guid applicationId, Guid nodeId, Guid userId)
        {
            return DataProvider.IsComplexAdmin(applicationId, nodeId, userId);
        }

        public static Guid? get_complex_type_id(Guid applicationId, Guid listId)
        {
            Guid? ret = DataProvider.GetComplexTypeID(applicationId, listId);
            if (ret == Guid.Empty) ret = null;
            return ret;
        }

        public static List<HierarchyAdmin> get_area_admins(Guid applicationId, 
            Guid nodeId, ref Node node, Service service = null)
        {
            if (service == null) service = CNController.get_service(applicationId, 
                node != null && node.NodeTypeID.HasValue ? node.NodeTypeID.Value : nodeId);
            if (service == null) return new List<HierarchyAdmin>();

            switch (service.AdminType)
            {
                case ServiceAdminType.AreaAdmin:
                case ServiceAdminType.ComplexAdmin:
                    if (node == null) node = CNController.get_node(applicationId, nodeId);

                    if (node == null || !node.AdminAreaID.HasValue)
                        return new List<HierarchyAdmin>();
                    else if (service.AdminType == ServiceAdminType.ComplexAdmin)
                        return CNController.get_complex_admins(applicationId, node.AdminAreaID.Value)
                            .Select(u => new HierarchyAdmin() { User = new User() { UserID = u }, Level = 0 }).ToList();
                    else
                        return CNController.get_hierarchy_admins(applicationId, node.AdminAreaID.Value).Where(
                            u => !service.MaxAcceptableAdminLevel.HasValue || service.MaxAcceptableAdminLevel < 0 ||
                                u.Level <= service.MaxAcceptableAdminLevel).Select(u => new HierarchyAdmin()
                                {
                                    User = new User() { UserID = u.User.UserID.Value },
                                    Level = u.Level
                                }).ToList();
                case ServiceAdminType.SpecificNode:
                    return !service.AdminNode.NodeID.HasValue ? new List<HierarchyAdmin>() :
                        CNController.get_members(applicationId, service.AdminNode.NodeID.Value,
                            pending: false, admin: true).Select(u => new HierarchyAdmin()
                            {
                                User = new User() { UserID = u.Member.UserID.Value },
                                Level = 0
                            }).ToList();
                case ServiceAdminType.Registerer:
                    if (node == null) node = CNController.get_node(applicationId, nodeId);
                    List<Guid> retList = new List<Guid>();
                    if (node != null && node.Creator.UserID.HasValue) retList.Add(node.Creator.UserID.Value);
                    return retList.Select(u => new HierarchyAdmin() { User = new User() { UserID = u }, Level = 0 }).ToList();
            }

            return new List<HierarchyAdmin>();
        }

        public static List<HierarchyAdmin> get_node_admins(Guid applicationId, 
            Guid nodeId, Node nodeObject = null, Service service = null)
        {
            if (nodeObject == null && (nodeObject = CNController.get_node(applicationId, nodeId)) == null)
                nodeObject = new Node();
            if (service == null && (service = CNController.get_service(applicationId, 
                nodeObject != null && nodeObject.NodeTypeID.HasValue ? nodeObject.NodeTypeID.Value : nodeId)) == null)
                service = new Service();
            
            List<HierarchyAdmin> admins = new List<HierarchyAdmin>();

            switch (service.AdminType)
            {
                case ServiceAdminType.AreaAdmin:
                    if (nodeObject.AdminAreaID.HasValue)
                        admins = CNController.get_area_admins(applicationId, nodeId, ref nodeObject);
                    break;
                case ServiceAdminType.ComplexAdmin:
                    if (nodeObject.AdminAreaID.HasValue)
                        admins = CNController.get_complex_admins(applicationId, nodeObject.AdminAreaID.Value)
                            .Select(u => new HierarchyAdmin() { User = new User() { UserID = u }, Level = 0 }).ToList(); 
                    break;
                case ServiceAdminType.SpecificNode:
                    if (service.AdminNode.NodeID.HasValue)
                    {
                        admins = CNController.get_members(applicationId, service.AdminNode.NodeID.Value,
                            pending: false, admin: true).Select(u => new HierarchyAdmin()
                            {
                                User = new User() { UserID = u.Member.UserID.Value },
                                Level = 0
                            }).ToList();
                    }
                    break;
                case ServiceAdminType.Registerer:
                    if (nodeObject.Creator.UserID.HasValue) admins.Add(new HierarchyAdmin()
                    {
                        User = new User() { UserID = nodeObject.Creator.UserID.Value },
                        Level = 0
                    });
                    break;
            }

            return admins;
        }

        public static bool is_node_admin(Guid applicationId, Guid userId, Guid nodeId, Guid? nodeTypeId, 
            Guid? areaId, bool? isCreator, Service service = null, List<NodeCreator> contributors = null)
        {
            if (service == null)
                service = CNController.get_service(applicationId, nodeTypeId.HasValue ? nodeTypeId.Value : nodeId);
            if (service == null) service = new Service();

            if (!areaId.HasValue && 
                (service.AdminType == ServiceAdminType.AreaAdmin || service.AdminType == ServiceAdminType.ComplexAdmin))
            {
                Node nd = CNController.get_node(applicationId, nodeId);
                if (nd != null) areaId = nd.AdminAreaID;
            }
            
            switch (service.AdminType)
            {
                case ServiceAdminType.AreaAdmin:
                    if (!areaId.HasValue) return false;
                    else
                    {
                        if (contributors == null) contributors = CNController.get_node_creators(applicationId, nodeId);

                        return service.MaxAcceptableAdminLevel.HasValue ? 
                            CNController.is_hierarchy_admin(applicationId, areaId.Value, userId,
                            contributors.Select(u => u.User.UserID.Value).ToList(), service.MaxAcceptableAdminLevel.Value) :
                            CNController.is_hierarchy_admin(applicationId, areaId.Value, userId, contributors.Select(u => u.User.UserID.Value).ToList());
                    }
                case ServiceAdminType.ComplexAdmin:
                    return !areaId.HasValue ? false : CNController.is_complex_admin(applicationId, areaId.Value, userId);
                case ServiceAdminType.SpecificNode:
                    return !service.AdminNode.NodeID.HasValue ? false :
                        CNController.is_admin_member(applicationId, service.AdminNode.NodeID.Value, userId);
                case ServiceAdminType.Registerer:
                    return isCreator.HasValue ? isCreator.Value : 
                        CNController.is_node_creator(applicationId, nodeId, userId);
            }

            return false;
        }

        public static bool get_user2node_status(Guid applicationId, Guid userId, Guid nodeId, 
            ref bool isCreator, ref bool isContributor, ref bool isExpert, ref bool isMember, ref bool isAdminMember, 
            ref bool isServiceAdmin, ref bool isAreaAdmin, ref bool editable, ref bool noOptionSet, 
            ref bool editSuggestion, Service service = null, List<NodeCreator> contributors = null)
        {
            //noOptionSet: All edit permissions are not set

            Guid? nodeTypeId = null;
            Guid? areaId = null;

            if (!DataProvider.GetUser2NodeStatus(applicationId, userId, nodeId, ref nodeTypeId, ref areaId, 
                ref isCreator, ref isContributor, ref isExpert, ref isMember, ref isAdminMember, ref isServiceAdmin))
            {
                editSuggestion = false;
                return false;
            }

            if (service == null) service = CNController.get_service(applicationId, nodeTypeId.Value);
            if (service == null) service = new Service();

            isAreaAdmin = CNController.is_node_admin(applicationId, userId, nodeId, nodeTypeId.Value, 
                areaId, isCreator, service, contributors);

            noOptionSet = !service.EditableForAdmin.HasValue && !service.EditableForCreator.HasValue && 
                !service.EditableForContributors.HasValue && !service.EditableForExperts.HasValue && 
                !service.EditableForMembers.HasValue;

            if (!service.EditableForAdmin.HasValue) service.EditableForAdmin = false;
            if (!service.EditableForCreator.HasValue) service.EditableForCreator = false;
            if (!service.EditableForContributors.HasValue) service.EditableForContributors = false;
            if (!service.EditableForExperts.HasValue) service.EditableForExperts = false;
            if (!service.EditableForMembers.HasValue) service.EditableForMembers = false;

            editable = PublicMethods.is_system_admin(applicationId, userId) || isServiceAdmin || 
                (isAreaAdmin && service.EditableForAdmin.Value) || (isCreator && service.EditableForCreator.Value) || 
                (isContributor && service.EditableForContributors.Value) || 
                (isExpert && service.EditableForExperts.Value) || (isMember && service.EditableForMembers.Value);

            editSuggestion = !service.EditSuggestion.HasValue || service.EditSuggestion.Value;

            return true;
        }

        public static bool get_user2node_status(Guid applicationId, Guid userId, Guid nodeId, 
            ref bool isCreator, ref bool isContributor, ref bool isExpert, ref bool isMember, 
            ref bool isAdminMember, ref bool isServiceAdmin, ref bool isAreaAdmin, 
            ref bool editable, Service service = null, List<NodeCreator> contributors = null)
        {
            bool noOptionSet = false;
            bool editSuggestion = false;

            return get_user2node_status(applicationId, userId, nodeId, ref isCreator, ref isContributor,
                ref isExpert, ref isMember, ref isAdminMember, ref isServiceAdmin, ref isAreaAdmin,
                ref editable, ref noOptionSet, ref editSuggestion, service, contributors);
        }

        public static bool has_edit_permission(Guid applicationId, 
            Guid userId, Guid nodeId, bool defaultPermissionForExperts, ref bool editSuggestion)
        {
            bool isCreator = false, isContributor = false, isExpert = false, isMember = false, isAdminMember = false,
                isServiceAdmin = false, isAreaAdmin = false, editable = false, noOptionSet = false;

            get_user2node_status(applicationId, userId, nodeId, ref isCreator, ref isContributor, ref isExpert, 
                ref isMember, ref isAdminMember, ref isServiceAdmin, ref isAreaAdmin, ref editable,
                ref noOptionSet, ref editSuggestion);

            return editable || (noOptionSet && defaultPermissionForExperts && isExpert) ||
                PrivacyController.check_access(applicationId, userId, nodeId, PrivacyObjectType.Node, PermissionType.Modify);
        }

        public static bool has_edit_permission(Guid applicationId, 
            Guid userId, Guid nodeId, bool defaultPermissionForExperts)
        {
            bool editSuggestion = false;
            return CNController.has_edit_permission(applicationId, 
                userId, nodeId, defaultPermissionForExperts, ref editSuggestion);
        }

        public static bool has_edit_permission(Guid applicationId, Guid userId, Guid nodeId)
        {
            return has_edit_permission(applicationId, userId, nodeId, false);
        }

        public static List<Guid> get_users_with_edit_permission(Guid applicationId, 
            Guid nodeId, bool defaultPermissionForExperts, ref bool editSuggestion)
        {
            List<Guid> retList = new List<Guid>();

            Service service = CNController.get_service(applicationId, nodeId);

            editSuggestion = service == null || !service.EditSuggestion.HasValue || service.EditSuggestion.Value;

            if (defaultPermissionForExperts && (
                    service == null || (
                        !service.EditableForAdmin.HasValue && !service.EditableForCreator.HasValue &&
                        !service.EditableForContributors.HasValue && !service.EditableForExperts.HasValue &&
                        !service.EditableForMembers.HasValue
                    )
                )
            )
            {
                return CNController.get_experts(applicationId, nodeId).Select(u => u.User.UserID.Value).ToList();
            }
            else if (service == null) return new List<Guid>();

            Node node = null;

            if (service.EditableForAdmin.HasValue && service.EditableForAdmin.Value)
                CNController.get_area_admins(applicationId, nodeId, ref node, service);

            if (service.EditableForContributors.HasValue && service.EditableForContributors.Value)
                retList.AddRange(CNController.get_node_creators(applicationId, nodeId).Select(u => u.User.UserID.Value));

            if (service.EditableForCreator.HasValue && service.EditableForCreator.Value)
                retList.Add(CNController.get_node(applicationId, nodeId).Creator.UserID.Value);

            if (service.EditableForExperts.HasValue && service.EditableForExperts.Value)
                retList.AddRange(CNController.get_experts(applicationId, nodeId).Select(u => u.User.UserID.Value));

            if (service.EditableForMembers.HasValue && service.EditableForMembers.Value)
                retList.AddRange(CNController.get_members(applicationId, nodeId, pending: false, admin: null)
                    .Select(u => u.Member.UserID.Value));

            return retList.Distinct().ToList();
        }

        public static List<Guid> get_users_with_edit_permission(Guid applicationId, 
            Guid nodeId, bool defaultPermissionForExperts)
        {
            bool editSuggestion = false;
            return CNController.get_users_with_edit_permission(applicationId,
                nodeId, defaultPermissionForExperts, ref editSuggestion);
        }

        public static List<Guid> get_users_with_edit_permission(Guid applicationId, Guid nodeId)
        {
            return get_users_with_edit_permission(applicationId, nodeId, false);
        }

        public static List<Hierarchy> get_node_hierarchy(Guid applicationId, Guid nodeId, bool? sameType = true)
        {
            List<Hierarchy> retList = new List<Hierarchy>();
            DataProvider.GetNodeHierarchy(applicationId, ref retList, nodeId, sameType);
            return retList;
        }

        public static List<Hierarchy> get_node_types_hierarchy(Guid applicationId, List<Guid> nodeTypeIds)
        {
            List<Hierarchy> retList = new List<Hierarchy>();
            DataProvider.GetNodeTypesHierarchy(applicationId, ref retList, nodeTypeIds);
            return retList;
        }

        public static List<Hierarchy> get_node_type_hierarchy(Guid applicationId, Guid nodeTypeId)
        {
            return get_node_types_hierarchy(applicationId, new List<Guid>() { nodeTypeId });
        }

        public static int get_tree_depth(Guid applicationId, Guid nodeTypeId)
        {
            return DataProvider.GetTreeDepth(applicationId, nodeTypeId);
        }

        public static List<HierarchyAdmin> get_hierarchy_admins(Guid applicationId, Guid nodeId, bool? sameType = true)
        {
            List<HierarchyAdmin> retList = new List<HierarchyAdmin>();
            DataProvider.GetHierarchyAdmins(applicationId, ref retList, nodeId, sameType);
            return retList;
        }

        public static bool is_hierarchy_admin(Guid applicationId, 
            Guid nodeId, Guid userId, List<Guid> contributorUserIds, int maxLevel = 2)
        {
            List<HierarchyAdmin> admins = get_hierarchy_admins(applicationId, nodeId).Where(
                u => maxLevel < 0 || u.Level <= maxLevel).OrderBy(v => v.Level).ToList();

            if (admins.Count == 0) return false;

            if (admins.Any(u => u.User.UserID == userId) && contributorUserIds.Any(u => u == userId) &&
                admins.Any(u => u.User.UserID != userId && !contributorUserIds.Any(x => x == u.User.UserID)))
                return false;

            for (int i = 0, lnt = admins.Count; i < lnt; ++i)
                if (admins[i].User.UserID == userId && !contributorUserIds.Any(u => u == userId)) return true;

            return admins.Any(u => u.User.UserID == userId);
        }

        public static bool is_hierarchy_admin(Guid applicationId, Guid nodeId, Guid userId, int maxLevel = 2)
        {
            return is_hierarchy_admin(applicationId, nodeId, userId, maxLevel);
        }

        public static List<NodeMember> get_members(Guid applicationId, 
            List<Guid> nodeIds, bool? pending, bool? admin, string searchText, int? count, long? lowerBoundary, ref long totalCount)
        {
            List<NodeMember> retList = new List<NodeMember>();
            DataProvider.GetMembers(applicationId, ref retList, nodeIds,
                pending, admin, searchText, count, lowerBoundary, ref totalCount);
            return retList;
        }

        public static List<NodeMember> get_members(Guid applicationId, List<Guid> nodeIds, bool? pending, bool? admin)
        {
            long totalCount = 0;
            return CNController.get_members(applicationId, nodeIds, pending, admin, null, null, null, ref totalCount);
        }

        public static List<NodeMember> get_members(Guid applicationId, 
            Guid nodeId, bool? pending, bool? admin, string searchText, int? count, long? lowerBoundary, ref long totalCount)
        {
            List<Guid> nIds = new List<Guid>();
            nIds.Add(nodeId);
            return CNController.get_members(applicationId, nIds, pending, admin, searchText, count, lowerBoundary, ref totalCount);
        }

        public static List<NodeMember> get_members(Guid applicationId, Guid nodeId, bool? pending, bool? admin)
        {
            long totalCount = 0;
            return CNController.get_members(applicationId, nodeId, pending, admin, null, null, null, ref totalCount);
        }

        public static NodeMember get_member(Guid applicationId, Guid nodeId, Guid userId)
        {
            return DataProvider.GetMember(applicationId, nodeId, userId);
        }

        public static List<Guid> get_member_user_ids(Guid applicationId, 
            ref List<Guid> nodeIds, NodeMemberStatuses? status = null, bool? admin = null)
        {
            List<Guid> retList = new List<Guid>();
            DataProvider.GetMemberUserIDs(applicationId, ref retList, ref nodeIds, status, admin);
            return retList;
        }

        public static List<Guid> get_member_user_ids(Guid applicationId, 
            Guid nodeId, NodeMemberStatuses? status = null, bool? admin = null)
        {
            List<Guid> _uIds = new List<Guid>();
            _uIds.Add(nodeId);
            return get_member_user_ids(applicationId, ref _uIds, status, admin);
        }

        public static int get_members_count(Guid applicationId, 
            Guid nodeId, NodeMemberStatuses? status = null, bool? admin = null)
        {
            return DataProvider.GetMembersCount(applicationId, nodeId, status, admin);
        }

        public static List<NodeMember> get_membership_requests_dashboards(Guid applicationId, Guid userId)
        {
            List<NodeMember> retList = new List<NodeMember>();
            DataProvider.GetMembershipRequestsDashboards(applicationId, ref retList, userId);
            return retList;
        }

        public static int get_membership_requests_dashboards_count(Guid applicationId, Guid userId)
        {
            return DataProvider.GetMembershipRequestsDashboardsCount(applicationId, userId);
        }

        public static List<NodesCount> get_membership_domains_count(Guid applicationId, Guid userId, Guid? nodeTypeId,
            Guid? nodeId, string additionalId, DateTime? lowerDateLimit, DateTime? upperDateLimit)
        {
            List<NodesCount> retList = new List<NodesCount>();
            DataProvider.GetMembershipDomainsCount(applicationId, ref retList, userId, nodeTypeId, 
                nodeId, additionalId, lowerDateLimit, upperDateLimit);
            return retList;
        }

        public static NodesCount get_membership_domains_count(Guid applicationId, Guid userId, Guid nodeTypeId,
            DateTime? lowerDateLimit, DateTime? upperDateLimit)
        {
            return CNController.get_membership_domains_count(applicationId, 
                userId, nodeTypeId, null, null, lowerDateLimit, upperDateLimit).FirstOrDefault();
        }

        public static List<Node> get_membership_domains(Guid applicationId, Guid userId, List<Guid> nodeTypeIds, 
            Guid? nodeId, string additionalId, string searchText, DateTime? lowerDateLimit, 
            DateTime? upperDateLimit, int? lowerBoundary, int? count, ref long totalCount)
        {
            List<Node> retList = new List<Node>();
            DataProvider.GetMembershipDomains(applicationId, ref retList, userId, nodeTypeIds, nodeId, 
                additionalId, searchText, lowerDateLimit, upperDateLimit, lowerBoundary, count, ref totalCount);
            return retList;
        }

        private static List<NodeMember> _get_member_nodes(Guid applicationId, List<Guid> userIds, 
            ref List<Guid> nodeTypeIds, NodeTypes? nodeType, bool? admin = null)
        {
            List<NodeMember> retList = new List<NodeMember>();
            DataProvider.GetMemberNodes(applicationId, ref retList, userIds, ref nodeTypeIds, nodeType, admin);
            return retList;
        }

        public static List<NodeMember> get_member_nodes(Guid applicationId, 
            Guid userId, ref List<Guid> nodeTypeIds, bool? admin = null)
        {
            List<Guid> userIds = new List<Guid>();
            userIds.Add(userId);
            return _get_member_nodes(applicationId, userIds, ref nodeTypeIds, null, admin);
        }

        public static List<NodeMember> get_member_nodes(Guid applicationId, 
            Guid userId, Guid nodeTypeId, bool? admin = null)
        {
            List<Guid> lst = new List<Guid>();
            lst.Add(nodeTypeId);
            List<Guid> userIds = new List<Guid>();
            userIds.Add(userId);
            return _get_member_nodes(applicationId, userIds, ref lst, null, admin);
        }

        public static List<NodeMember> get_member_nodes(Guid applicationId, 
            Guid userId, NodeTypes nodeType, bool? admin = null)
        {
            List<Guid> lst = new List<Guid>();
            List<Guid> userIds = new List<Guid>();
            userIds.Add(userId);
            return _get_member_nodes(applicationId, userIds, ref lst, nodeType, admin);
        }

        public static List<NodeMember> get_member_nodes(Guid applicationId, Guid userId, bool? admin = null)
        {
            List<Guid> lst = new List<Guid>();
            List<Guid> userIds = new List<Guid>();
            userIds.Add(userId);
            return _get_member_nodes(applicationId, userIds, ref lst, null, admin);
        }

        public static List<NodeMember> get_member_nodes(Guid applicationId, 
            List<Guid> userIds, List<Guid> nodeTypeIds, bool? admin = null)
        {
            return _get_member_nodes(applicationId, userIds, ref nodeTypeIds, null, admin);
        }

        public static List<Guid> get_child_hierarchy_member_ids(Guid applicationId,
            Guid nodeId, string searchText, int? count, long? lowerBoundary, ref long totalCount)
        {
            List<Guid> lst = new List<Guid>();
            DataProvider.GetChildHierarchyMemberIDs(applicationId, ref lst, nodeId, searchText, count, lowerBoundary, ref totalCount);
            return lst;
        }

        public static List<Guid> get_child_hierarchy_expert_ids(Guid applicationId,
            Guid nodeId, string searchText, int? count, long? lowerBoundary, ref long totalCount)
        {
            List<Guid> lst = new List<Guid>();
            DataProvider.GetChildHierarchyExpertIDs(applicationId, ref lst, nodeId, searchText, count, lowerBoundary, ref totalCount);
            return lst;
        }

        public static List<NodeMember> get_users_departments(Guid applicationId, List<Guid> userIds)
        {
            List<NodeMember> retList = new List<NodeMember>();
            DataProvider.GetUsersDepartments(applicationId, ref retList, ref userIds);
            return retList;
        }

        public static NodeMember get_user_department(Guid applicationId, Guid userId)
        {
            List<Guid> _uIds = new List<Guid>();
            _uIds.Add(userId);
            return get_users_departments(applicationId, _uIds).FirstOrDefault();
        }

        public static bool like_nodes(Guid applicationId, ref List<Guid> nodeIds, Guid userId)
        {
            return DataProvider.LikeNodes(applicationId, ref nodeIds, userId);
        }

        public static bool like_nodes(Guid applicationId, List<Guid> nodeIds, Guid userId)
        {
            return like_nodes(applicationId, ref nodeIds, userId);
        }

        public static bool like_node(Guid applicationId, Guid nodeId, Guid userId)
        {
            List<Guid> _nIds = new List<Guid>();
            _nIds.Add(nodeId);
            return like_nodes(applicationId, ref _nIds, userId);
        }

        public static bool unlike_nodes(Guid applicationId, ref List<Guid> nodeIds, Guid userId)
        {
            return DataProvider.UnlikeNodes(applicationId, ref nodeIds, userId);
        }

        public static bool unlike_nodes(Guid applicationId, List<Guid> nodeIds, Guid userId)
        {
            return unlike_nodes(applicationId, ref nodeIds, userId);
        }

        public static bool unlike_node(Guid applicationId, Guid nodeId, Guid userId)
        {
            List<Guid> _nIds = new List<Guid>();
            _nIds.Add(nodeId);
            return unlike_nodes(applicationId, ref _nIds, userId);
        }

        public static List<Guid> is_fan(Guid applicationId, ref List<Guid> nodeIds, Guid userId)
        {
            List<Guid> retIds = new List<Guid>();
            DataProvider.IsFan(applicationId, ref retIds, ref nodeIds, userId);
            return retIds;
        }

        public static List<Guid> is_fan(Guid applicationId, List<Guid> nodeIds, Guid userId)
        {
            return is_fan(applicationId, ref nodeIds, userId);
        }

        public static bool is_fan(Guid applicationId, Guid nodeId, Guid userId)
        {
            List<Guid> _nIds = new List<Guid>();
            _nIds.Add(nodeId);
            Guid? n = is_fan(applicationId, ref _nIds, userId).FirstOrDefault();
            return n.HasValue && n == nodeId;
        }

        public static List<Guid> get_node_fans_user_ids(Guid applicationId, Guid nodeId, 
            int? count, long? lowerBoundary, ref long totalCount)
        {
            List<Guid> retList = new List<Guid>();
            DataProvider.GetNodeFansUserIDs(applicationId, ref retList, nodeId, count, lowerBoundary, ref totalCount);
            return retList;
        }

        public static List<Guid> get_node_fans_user_ids(Guid applicationId, 
            Guid nodeId, int? count = null, long? lowerBoundary = null)
        {
            long totalCount = 0;
            return CNController.get_node_fans_user_ids(applicationId, nodeId, count, lowerBoundary, ref totalCount);
        }

        public static List<User> get_node_fans(Guid applicationId, 
            Guid nodeId, int? count, long? lowerBoundary, ref long totalCount)
        {
            return UsersController.get_users(applicationId, CNController.get_node_fans_user_ids(applicationId,
                nodeId, count, lowerBoundary, ref totalCount));
        }

        public static List<NodesCount> get_favorite_nodes_count(Guid applicationId, Guid userId, Guid? nodeTypeId,
            Guid? nodeId, string additionalId, bool? isDocument, DateTime? lowerDateLimit, DateTime? upperDateLimit)
        {
            List<NodesCount> retList = new List<NodesCount>();
            DataProvider.GetFavoriteNodesCount(applicationId, ref retList, userId, 
                nodeTypeId, nodeId, additionalId, isDocument, lowerDateLimit, upperDateLimit);
            return retList;
        }

        public static NodesCount get_favorite_nodes_count(Guid applicationId, Guid userId, Guid nodeTypeId,
            bool? isDocument, DateTime? lowerDateLimit, DateTime? upperDateLimit)
        {
            return CNController.get_favorite_nodes_count(applicationId,
                userId, nodeTypeId, null, null, isDocument, lowerDateLimit, upperDateLimit).FirstOrDefault();
        }

        public static List<Node> get_favorite_nodes(Guid applicationId, Guid userId, List<Guid> nodeTypeIds, 
            bool? useNodeTypeHierarchy, Guid? nodeId, string additionalId, string searchText, bool? isDocument,
            Guid? creatorUserId, Guid? relatedToNodeId, List<FormFilter> filters, bool? matchAllFilters,
            DateTime? lowerDateLimit, DateTime? upperDateLimit, int? lowerBoundary, int? count, ref long totalCount)
        {
            List<Node> retList = new List<Node>();
            DataProvider.GetFavoriteNodes(applicationId, ref retList, userId, nodeTypeIds, useNodeTypeHierarchy, nodeId, 
                additionalId, searchText, isDocument, creatorUserId, relatedToNodeId, filters, matchAllFilters, 
                lowerDateLimit, upperDateLimit, lowerBoundary, count, ref totalCount);
            return retList;
        }

        public static bool add_complex(Guid applicationId, NodeList Info)
        {
            return DataProvider.AddComplex(applicationId, Info);
        }

        public static bool modify_complex(Guid applicationId, NodeList Info)
        {
            return DataProvider.ModifyComplex(applicationId, Info);
        }

        public static bool remove_complexes(Guid applicationId, ref List<Guid> listIds, Guid lastModifierUserId)
        {
            return DataProvider.ArithmeticDeleteComplexes(applicationId, ref listIds, lastModifierUserId);
        }

        public static bool remove_complex(Guid applicationId, Guid listId, Guid lastModifierUserId)
        {
            List<Guid> _nIds = new List<Guid>();
            _nIds.Add(listId);
            return remove_complexes(applicationId, ref _nIds, lastModifierUserId);
        }

        public static bool add_complex_admin(Guid applicationId, Guid listId, Guid userId, Guid currentUserId)
        {
            return DataProvider.AddComplexAdmin(applicationId, listId, userId, currentUserId);
        }

        public static bool remove_complex_admin(Guid applicationId, Guid listId, Guid userId, Guid currentUserId)
        {
            return DataProvider.RemoveComplexAdmin(applicationId, listId, userId, currentUserId);
        }

        public static List<NodeList> get_lists(Guid applicationId, ref List<Guid> listIds)
        {
            List<NodeList> retList = new List<NodeList>();
            DataProvider.GetLists(applicationId, ref retList, ref listIds);
            return retList;
        }

        public static NodeList get_list(Guid applicationId, Guid listId)
        {
            List<Guid> _nIds = new List<Guid>();
            _nIds.Add(listId);
            return get_lists(applicationId, ref _nIds).FirstOrDefault();
        }
        
        private static List<NodeList> _get_lists(Guid applicationId, 
            Guid? nodeTypeId, NodeTypes? nodeType, string searchText, Guid? minId, int? count)
        {
            List<NodeList> retList = new List<NodeList>();
            DataProvider.GetLists(applicationId, ref retList, nodeTypeId, nodeType, searchText, count, minId);
            return retList;
        }

        public static List<NodeList> get_lists(Guid applicationId, 
            Guid nodeTypeId, string searchText = null, Guid? minId = null, int? count = 1000)
        {
            return CNController._get_lists(applicationId, nodeTypeId, null, searchText, minId, count);
        }

        public static List<NodeList> get_lists(Guid applicationId, 
            NodeTypes nodeType, string searchText = null, Guid? minId = null, int count = 1000)
        {
            return CNController._get_lists(applicationId, null, nodeType, searchText, minId, count);
        }


        public static bool add_nodes_to_complex(Guid applicationId, 
            Guid listId, ref List<Guid> nodeIds, Guid creatorUserId)
        {
            return DataProvider.AddNodesToComplex(applicationId, listId, ref nodeIds, creatorUserId);
        }

        public static bool add_node_to_complex(Guid applicationId, Guid listId, Guid nodeId, Guid creatorUserId)
        {
            List<Guid> _nIds = new List<Guid>();
            _nIds.Add(nodeId);
            return add_nodes_to_complex(applicationId, listId, ref _nIds, creatorUserId);
        }

        public static bool remove_complex_nodes(Guid applicationId, 
            Guid listId, ref List<Guid> nodeIds, Guid lastModifierUserId)
        {
            return DataProvider.ArithmeticDeleteComplexNodes(applicationId, listId, ref nodeIds, lastModifierUserId);
        }

        public static bool remove_complex_node(Guid applicationId, Guid listId, Guid nodeId, Guid lastModifierUserId)
        {
            List<Guid> _nIds = new List<Guid>();
            _nIds.Add(nodeId);
            return remove_complex_nodes(applicationId, listId, ref _nIds, lastModifierUserId);
        }

        private static List<Node> _get_list_nodes(Guid applicationId, 
            Guid listId, Guid? nodeTypeId, NodeTypes? nodeType)
        {
            List<Node> lstNodes = new List<Node>();
            DataProvider.GetListNodes(applicationId, ref lstNodes, listId, nodeTypeId, nodeType);
            return lstNodes;
        }

        public static List<Node> get_list_nodes(Guid applicationId, Guid listId)
        {
            return _get_list_nodes(applicationId, listId, null, null);
        }

        public static List<Node> get_list_nodes(Guid applicationId, Guid listId, Guid nodeTypeId)
        {
            return _get_list_nodes(applicationId, listId, nodeTypeId, null);
        }

        public static List<Node> get_list_nodes(Guid applicationId, Guid listId, NodeTypes nodeType)
        {
            return _get_list_nodes(applicationId, listId, null, nodeType);
        }
        
        private static void _save_tags(object obj)
        {
            DataProvider.AddTags((Guid)((Pair)(((Pair)obj).Second)).First,
                (List<Tag>)((Pair)obj).First, (Guid)((Pair)(((Pair)obj).Second)).Second);
        }

        public static void save_tags(Guid applicationId, List<string> tags, Guid currentUserId)
        {
            if (tags.Count == 0) return;
            List<Tag> tagObjects = new List<Tag>();
            foreach (string tg in tags)
                tagObjects.Add(new Tag() { Text = tg });
            Pair obj = new Pair(tagObjects, new Pair(applicationId, currentUserId));
            ThreadPool.QueueUserWorkItem(new WaitCallback(_save_tags), obj);
        }

        public static Guid? add_tag(Guid applicationId, Tag info)
        {
            List<Tag> lst = new List<Tag>();
            lst.Add(info);
            return DataProvider.AddTags(applicationId, lst, info.CreatorUserID);
        }

        public static List<Tag> search_tags(Guid applicationId, 
            string searchText, int? count = null, int? lowerBoundary = null)
        {
            List<Tag> tags = new List<Tag>();
            DataProvider.SearchTags(applicationId, ref tags, searchText, count, lowerBoundary);
            return tags;
        }

        public static List<NodeCreator> get_node_creators(Guid applicationId, Guid nodeId, bool? full = null)
        {
            List<NodeCreator> retList = new List<NodeCreator>();
            DataProvider.GetNodeCreators(applicationId, ref retList, nodeId, full);
            return retList;
        }

        private static List<Node> _get_creator_nodes(Guid applicationId, Guid userId, Guid? nodeTypeId)
        {
            List<Node> retList = new List<Node>();
            DataProvider.GetCreatorNodes(applicationId, ref retList, userId, nodeTypeId);
            return retList;
        }

        public static List<Node> get_creator_nodes(Guid applicationId, Guid userId)
        {
            return _get_creator_nodes(applicationId, userId, null);
        }

        public static List<Node> get_creator_nodes(Guid applicationId, Guid userId, Guid nodeTypeId)
        {
            return _get_creator_nodes(applicationId, userId, nodeTypeId);
        }

        public static bool add_experts(Guid applicationId, Guid nodeId, ref List<Guid> userIds)
        {
            return DataProvider.AddExperts(applicationId, nodeId, ref userIds);
        }

        public static bool add_expert(Guid applicationId, Guid nodeId, Guid userId)
        {
            List<Guid> uIds = new List<Guid>();
            uIds.Add(userId);
            return add_experts(applicationId, nodeId, ref uIds);
        }

        public static bool remove_experts(Guid applicationId, Guid nodeId, ref List<Guid> userIds)
        {
            return DataProvider.ArithmeticDeleteExperts(applicationId, nodeId, ref userIds);
        }

        public static bool remove_expert(Guid applicationId, Guid nodeId, Guid userId)
        {
            List<Guid> _uIds = new List<Guid>();
            _uIds.Add(userId);
            return remove_experts(applicationId, nodeId, ref _uIds);
        }

        public static List<Expert> get_experts(Guid applicationId, 
            ref List<Guid> nodeIds, string searchText, int? count, long? lowerBoundary, ref long totalCount, bool hierarchy = false)
        {
            List<Expert> retList = new List<Expert>();
            DataProvider.GetExperts(applicationId, ref retList, nodeIds, searchText, hierarchy, count, lowerBoundary, ref totalCount);
            return retList;
        }

        public static List<Expert> get_experts(Guid applicationId, ref List<Guid> nodeIds, bool hierarchy = false)
        {
            long totalCount = 0;
            return CNController.get_experts(applicationId, ref nodeIds, null, null, null, ref totalCount, hierarchy: hierarchy);
        }

        public static List<Expert> get_experts(Guid applicationId, 
            Guid nodeId, string searchText, int? count, long? lowerBoundary, ref long totalCount, bool hierarchy = false)
        {
            List<Guid> _nIds = new List<Guid>();
            _nIds.Add(nodeId);
            return CNController.get_experts(applicationId, ref _nIds, searchText, count, lowerBoundary, ref totalCount, hierarchy: hierarchy);
        }

        public static List<Expert> get_experts(Guid applicationId, Guid nodeId, bool hierarchy = false)
        {
            long totalCount = 0;
            return CNController.get_experts(applicationId, nodeId, null, null, null, ref totalCount, hierarchy: hierarchy);
        }

        public static List<NodesCount> get_expertise_domains_count(Guid applicationId, Guid userId, Guid? nodeTypeId,
            Guid? nodeId, string additionalId, DateTime? lowerDateLimit, DateTime? upperDateLimit)
        {
            List<NodesCount> retList = new List<NodesCount>();
            DataProvider.GetExpertiseDomainsCount(applicationId,
                ref retList, userId, nodeTypeId, nodeId, additionalId, lowerDateLimit, upperDateLimit);
            return retList;
        }

        public static NodesCount get_expertise_domains_count(Guid applicationId, Guid userId, Guid nodeTypeId,
            DateTime? lowerDateLimit, DateTime? upperDateLimit)
        {
            return CNController.get_expertise_domains_count(applicationId, 
                userId, nodeTypeId, null, null, lowerDateLimit, upperDateLimit).FirstOrDefault();
        }

        public static List<Node> get_expertise_domains(Guid applicationId, Guid userId, List<Guid> nodeTypeIds, 
            Guid? nodeId, string additionalId, string searchText, DateTime? lowerDateLimit, 
            DateTime? upperDateLimit, int? lowerBoundary, int? count, ref long totalCount)
        {
            List<Node> retList = new List<Node>();
            DataProvider.GetExpertiseDomains(applicationId, ref retList, userId, nodeTypeIds, nodeId, 
                additionalId, searchText, lowerDateLimit, upperDateLimit, lowerBoundary, count, ref totalCount);
            return retList;
        }

        public static List<Expert> get_expertise_domains(Guid applicationId, ref List<Guid> userIds, bool? approved, 
            bool? socialApproved, bool? all = null, Guid? nodeTypeId = null)
        {
            List<Expert> retList = new List<Expert>();
            DataProvider.GetExpertiseDomains(applicationId, ref retList, ref userIds, 
                nodeTypeId, approved, socialApproved, all);
            return retList;
        }

        public static List<Expert> get_expertise_domains(Guid applicationId, Guid userId, bool? approved, 
            bool? socialApproved, bool? all = null, Guid? nodeTypeId = null)
        {
            List<Guid> _uIds = new List<Guid>();
            _uIds.Add(userId);
            return get_expertise_domains(applicationId, ref _uIds, approved, socialApproved, all, nodeTypeId);
        }

        public static List<Guid> get_expertise_domain_ids(Guid applicationId, 
            ref List<Guid> userIds, bool? approved, bool? socialApproved)
        {
            List<Guid> retList = new List<Guid>();
            DataProvider.GetExpertiseDomainIDs(applicationId, ref retList, ref userIds, approved, socialApproved);
            return retList;
        }

        public static List<Guid> get_expertise_domain_ids(Guid applicationId, 
            Guid userId, bool? approved, bool? socialApproved)
        {
            List<Guid> _uIds = new List<Guid>();
            _uIds.Add(userId);
            return get_expertise_domain_ids(applicationId, ref _uIds, approved, socialApproved);
        }

        public static bool is_expert(Guid applicationId, 
            Guid userId, Guid nodeId, bool? approved, bool? socialApproved)
        {
            return DataProvider.IsExpert(applicationId, userId, nodeId, approved, socialApproved);
        }

        public static int get_experts_count(Guid applicationId, 
            bool? approved, bool? socialApproved, Guid? nodeId = null, bool? distinctUsers = null)
        {
            return DataProvider.GetExpertsCount(applicationId, nodeId, distinctUsers, approved, socialApproved);
        }

        public static bool vote_expertise(Guid applicationId, 
            Guid referrerUserId, Guid nodeId, Guid userId, bool? confirmStatus)
        {
            return DataProvider.VoteExpertise(applicationId, referrerUserId, nodeId, userId, confirmStatus);
        }

        public static Guid? i_am_expert(Guid applicationId, Guid userId, string expertiseDomain)
        {
            return DataProvider.IAmExpert(applicationId, userId, expertiseDomain);
        }

        public static bool i_am_not_expert(Guid applicationId, Guid userId, Guid nodeId)
        {
            return DataProvider.IAmNotExpert(applicationId, userId, nodeId);
        }

        public static int get_referrals_count(Guid applicationId, Guid userId, Guid nodeId)
        {
            return DataProvider.GetReferralsCount(applicationId, userId, nodeId);
        }

        public static List<Expert> get_expertise_suggestions(Guid applicationId, 
            Guid userId, int count, int? lowerBoundary)
        {
            List<Expert> retList = new List<Expert>();
            DataProvider.GetExpertiseSuggestions(applicationId, ref retList, userId, count, lowerBoundary);
            return retList;
        }

        public static List<Node> suggest_node_relations(Guid applicationId, 
            Guid userId, Guid? relatedNodeTypeId, int? count = 20)
        {
            List<Node> nodes = new List<Node>();
            DataProvider.SuggestNodeRelations(applicationId, ref nodes, userId, relatedNodeTypeId, count);
            return nodes;
        }

        public static List<NodeType> suggest_node_types_for_relations(Guid applicationId, Guid userId, int? count = 10)
        {
            List<NodeType> nodeTypes = new List<NodeType>();
            DataProvider.SuggestNodeTypesForRelations(applicationId, ref nodeTypes, userId, count);
            return nodeTypes;
        }

        public static List<SimilarNode> suggest_similar_nodes(Guid applicationId, Guid nodeId, int? count)
        {
            List<SimilarNode> lst = new List<CoreNetwork.SimilarNode>();
            DataProvider.SuggestSimilarNodes(applicationId, ref lst, nodeId, count);
            if (lst == null || lst.Count == 0) return lst;

            List<Node> nds = get_nodes(applicationId, lst.Select(u => u.Suggested.NodeID.Value).ToList(), full: null);

            for (int i = 0; i < lst.Count; ++i)
                lst[i].Suggested = nds.Where(u => u.NodeID == lst[i].Suggested.NodeID).FirstOrDefault();

            return lst;
        }

        public static List<KnowledgableUser> suggest_knowledgable_users(Guid applicationId, Guid nodeId, int? count)
        {
            List<KnowledgableUser> lst = new List<CoreNetwork.KnowledgableUser>();
            DataProvider.SuggestKnowledgableUsers(applicationId, ref lst, nodeId, count);
            if (lst == null || lst.Count == 0) return lst;

            List<User> usrs = UsersController.get_users(applicationId, lst.Select(u => u.User.UserID.Value).ToList());

            for (int i = 0; i < lst.Count; ++i)
                lst[i].User = usrs.Where(u => u.UserID == lst[i].User.UserID).FirstOrDefault();

            return lst;
        }

        public static List<Guid> get_existing_node_ids(Guid applicationId, List<Guid> nodeIds, 
            bool? searchable, bool? noContent)
        {
            List<Guid> retIds = new List<Guid>();

            PublicMethods.split_list<Guid>(nodeIds, 200, ids =>
            {
                List<Guid> newNodeIds = new List<Guid>();

                DataProvider.GetExistingNodeIDs(applicationId, ref newNodeIds, ref ids, searchable, noContent);

                if (newNodeIds.Count > 0) retIds.AddRange(newNodeIds);
            });

            return retIds;
        }

        public static List<Guid> get_existing_node_type_ids(Guid applicationId, List<Guid> nodeTypeIds, bool? noContent)
        {
            List<Guid> retIds = new List<Guid>();
            DataProvider.GetExistingNodeTypeIDs(applicationId, ref retIds, ref nodeTypeIds, noContent);
            return retIds;
        }

        public static List<NodeInfo> get_node_info(Guid applicationId, List<Guid> nodeIds, Guid? currentUserId, 
            bool? tags, bool? description, bool? creator, bool? contributorsCount, bool? likesCount, bool? visitsCount, 
            bool? expertsCount, bool? membersCount, bool? childsCount, bool? relatedNodesCount, bool? likeStatus)
        {
            List<NodeInfo> retList = new List<NodeInfo>();
            DataProvider.GetNodeInfo(applicationId, ref retList, nodeIds, currentUserId, tags, description, 
                creator, contributorsCount, likesCount, visitsCount, expertsCount, membersCount, 
                childsCount, relatedNodesCount, likeStatus);
            return retList;
        }

        public static NodeInfo get_node_info(Guid applicationId, Guid nodeId, Guid? currentUserId, 
            bool? tags, bool? description, bool? creator, bool? contributorsCount, bool? likesCount, bool? visitsCount, 
            bool? expertsCount, bool? membersCount, bool? childsCount, bool? relatedNodesCount, bool? likeStatus)
        {
            List<Guid> nIds = new List<Guid>();
            nIds.Add(nodeId);
            return CNController.get_node_info(applicationId, nIds, currentUserId, tags, description, creator, 
                contributorsCount, likesCount, visitsCount, expertsCount, membersCount, childsCount, 
                relatedNodesCount, likeStatus).FirstOrDefault();
        }

        public static bool initialize_extensions(Guid applicationId, Guid ownerId, Guid currentUserId, bool ignoreDefault = false)
        {
            return DataProvider.InitializeExtensions(applicationId, ownerId, currentUserId, ignoreDefault);
        }

        public static bool enable_extension(Guid applicationId, 
            Guid ownerId, ExtensionType extensionType, Guid currentUserId)
        {
            return DataProvider.EnableDisableExtension(applicationId, ownerId, extensionType, false, currentUserId);
        }

        public static bool disable_extension(Guid applicationId, 
            Guid ownerId, ExtensionType extensionType, Guid currentUserId)
        {
            return DataProvider.EnableDisableExtension(applicationId, ownerId, extensionType, true, currentUserId);
        }

        public static bool set_extension_title(Guid applicationId, 
            Guid ownerId, ExtensionType extensionType, string title, Guid currentUserId)
        {
            return DataProvider.SetExtensionTitle(applicationId, ownerId, extensionType, title, currentUserId);
        }

        public static bool move_extension(Guid applicationId, 
            Guid ownerId, ExtensionType extensionType, bool moveDown)
        {
            return DataProvider.MoveExtension(applicationId, ownerId, extensionType, moveDown);
        }

        public static bool save_extensions(Guid applicationId, Guid ownerId, List<Extension> extensions, Guid currentUserId)
        {
            return DataProvider.SaveExtensions(applicationId, ownerId, extensions, currentUserId);
        }

        public static List<Extension> get_extensions(Guid applicationId, Guid ownerId)
        {
            List<Extension> retList = new List<Extension>();
            DataProvider.GetExtensions(applicationId, ref retList, ownerId);
            return retList;
        }

        public static bool has_extension(Guid applicationId, Guid ownerId, ExtensionType extensionType)
        {
            return DataProvider.HasExtension(applicationId, ownerId, extensionType);
        }

        public static List<NodeType> get_node_types_with_extension(Guid applicationId, List<ExtensionType> exts)
        {
            List<NodeType> lst = new List<NodeType>();
            DataProvider.GetNodeTypesWithExtension(applicationId, ref lst, exts);
            return lst;
        }

        public static List<NodeType> get_node_types_with_extension(Guid applicationId, ExtensionType ext)
        {
            return get_node_types_with_extension(applicationId, new List<ExtensionType>() { ext });
        }

        public static List<NodesCount> get_intellectual_properties_count(Guid applicationId, Guid userId, 
            Guid? nodeTypeId, Guid? nodeId, string additionalId, Guid? currentUserId, bool? isDocument,
            DateTime? lowerDateLimit, DateTime? upperDateLimit)
        {
            List<NodesCount> retList = new List<NodesCount>();
            DataProvider.GetIntellectualPropertiesCount(applicationId, ref retList, 
                userId, nodeTypeId, nodeId, additionalId, currentUserId, isDocument, lowerDateLimit, upperDateLimit);
            return retList;
        }

        public static NodesCount get_intellectual_properties_count(Guid applicationId, Guid userId, Guid nodeTypeId,
            Guid? currentUserId, bool? isDocument, DateTime? lowerDateLimit, DateTime? upperDateLimit)
        {
            return CNController.get_intellectual_properties_count(applicationId,
                userId, nodeTypeId, null, null, currentUserId, isDocument, lowerDateLimit, upperDateLimit).FirstOrDefault();
        }

        public static List<Node> get_intellectual_properties(Guid applicationId, Guid userId, List<Guid> nodeTypeIds, 
            Guid? nodeId, string additionalId, Guid? currentUserId, string searchText, bool? isDocument,
            DateTime? lowerDateLimit, DateTime? upperDateLimit, int? lowerBoundary, int? count, ref long totalCount)
        {
            List<Node> retList = new List<Node>();
            DataProvider.GetIntellectualProperties(applicationId, ref retList, userId, nodeTypeIds, nodeId, 
                additionalId, currentUserId, searchText, isDocument, lowerDateLimit, upperDateLimit, lowerBoundary, count, ref totalCount);
            return retList;
        }

        public static List<Node> get_intellectual_properties_of_friends(Guid applicationId, Guid userId, 
            Guid? nodeTypeId, int? lowerBoundary, int? count)
        {
            List<Node> retList = new List<Node>();
            DataProvider.GetIntellectualPropertiesOfFriends(applicationId,
                ref retList, userId, nodeTypeId, lowerBoundary, count);
            return retList;
        }

        public static List<Node> get_document_tree_node_items(Guid applicationId, 
            Guid documentTreeNodeId, Guid? currenrUserId, bool? checkPrivacy, int? count, int? lowerBoundary)
        {
            List<Node> retList = new List<Node>();
            DataProvider.GetDocumentTreeNodeItems(applicationId,
                ref retList, documentTreeNodeId, currenrUserId, checkPrivacy, count, lowerBoundary);
            return retList;
        }

        public static List<Node> get_document_tree_node_contents(Guid applicationId, Guid documentTreeNodeId, 
            Guid? currenrUserId, bool? checkPrivacy, int? count, int? lowerBoundary, string searchText, ref long totalCount)
        {
            List<Node> retList = new List<Node>();
            DataProvider.GetDocumentTreeNodeContents(applicationId, ref retList, documentTreeNodeId, 
                currenrUserId, checkPrivacy, count, lowerBoundary, searchText, ref totalCount);
            return retList;
        }

        public static List<Guid> is_node_type(Guid applicationId, List<Guid> ids)
        {
            return DataProvider.IsNodeType(applicationId, ids);
        }

        public static bool is_node_type(Guid applicationId, Guid id)
        {
            return is_node_type(applicationId, new List<Guid>() { id }).Count == 1;
        }

        public static List<Guid> is_node(Guid applicationId, List<Guid> ids)
        {
            return DataProvider.IsNode(applicationId, ids);
        }

        public static bool is_node(Guid applicationId, Guid id)
        {
            return is_node(applicationId, new List<Guid>() { id }).Count == 1;
        }

        public static List<ExploreItem> explore(Guid applicationId, Guid? baseId, Guid? relatedId,
            List<Guid> baseTypeIds, List<Guid> relatedTypeIds, Guid? secondLevelNodeId, 
            bool? registrationArea, bool? tags, bool? relations, int? lowerBoundary, int? count, string orderBy, 
            bool? orderByDesc, string searchText, bool? checkAccess, Guid? currentUserId, ref long totalCount)
        {
            List<ExploreItem> lst = new List<CoreNetwork.ExploreItem>();
            DataProvider.Explore(applicationId, ref lst, baseId, relatedId, baseTypeIds, relatedTypeIds,
                secondLevelNodeId, registrationArea, tags, relations, lowerBoundary, count, orderBy, orderByDesc, 
                searchText, checkAccess, currentUserId, ref totalCount);
            return lst;
        }

        public static bool update_form_and_wiki_tags(Guid applicationId, int? count)
        {
            return DataProvider.UpdateFormAndWikiTags(applicationId, new List<Guid>(), null, count);
        }

        public static bool update_form_and_wiki_tags(Guid applicationId, List<Guid> nodeIds, Guid currentUserId)
        {
            return nodeIds != null && nodeIds.Count > 0 && currentUserId != Guid.Empty &&
                DataProvider.UpdateFormAndWikiTags(applicationId, nodeIds, currentUserId, null);
        }

        public static bool update_form_and_wiki_tags(Guid applicationId, Guid nodeId, Guid currentUserId)
        {
            return update_form_and_wiki_tags(applicationId, new List<Guid>() { nodeId }, currentUserId);
        }

        /* Service */

        public static bool initialize_service(Guid applicationId, Guid nodeTypeId)
        {
            return DataProvider.InitializeService(applicationId, nodeTypeId);
        }

        private static List<Service> _get_services(Guid applicationId, 
            Guid? nodeTypeId, Guid? currentUserId, bool? isDocument, bool? isKnowledge, bool? checkPrivacy)
        {
            List<Service> retList = new List<Service>();
            DataProvider.GetServices(applicationId, ref retList, 
                nodeTypeId, currentUserId, isDocument, isKnowledge, checkPrivacy);
            return retList;
        }

        public static List<Service> get_services(Guid applicationId, 
            Guid currentUserId, bool? isDocument = null, bool? isKnowledge = null, bool? checkPrivacy = true)
        {
            return _get_services(applicationId, null, currentUserId, isDocument, isKnowledge, checkPrivacy);
        }

        public static List<Service> get_services(Guid applicationId, List<Guid> nodeTypeIds)
        {
            List<Service> services = new List<Service>();
            DataProvider.GetServices(applicationId, ref services, nodeTypeIds);
            return services;
        }

        public static Service get_service(Guid applicationId, Guid nodeTypeIdOrNodeId)
        {
            return _get_services(applicationId, nodeTypeIdOrNodeId, null, null, null, false).FirstOrDefault();
        }

        public static bool set_service_title(Guid applicationId, Guid nodeTypeId, string title)
        {
            return DataProvider.SetServiceTitle(applicationId, nodeTypeId, title);
        }

        public static bool set_service_description(Guid applicationId, Guid nodeTypeId, string description)
        {
            return DataProvider.SetServiceDescription(applicationId, nodeTypeId, description);
        }

        public static bool set_service_success_message(Guid applicationId, Guid nodeTypeId, string message)
        {
            return DataProvider.SetServiceSuccessMessage(applicationId, nodeTypeId, message);
        }

        public static string get_service_success_message(Guid applicationId, Guid nodeTypeId)
        {
            return DataProvider.GetServiceSuccessMessage(applicationId, nodeTypeId);
        }

        public static bool set_service_admin_type(Guid applicationId, Guid serviceTypeId, ServiceAdminType adminType,
            Guid? adminNodeId, ref List<Guid> limitNodeTypeIds, Guid currentUserId)
        {
            return DataProvider.SetServiceAdminType(applicationId,
                serviceTypeId, adminType, adminNodeId, ref limitNodeTypeIds, currentUserId);
        }

        public static List<NodeType> get_admin_area_limits(Guid applicationId, Guid nodeTypeIdOrNodeId)
        {
            List<NodeType> retList = new List<NodeType>();
            DataProvider.GetAdminAreaLimits(applicationId, ref retList, nodeTypeIdOrNodeId);
            return retList;
        }

        public static bool set_max_acceptable_admin_level(Guid applicationId, 
            Guid nodeTypeId, int? maxAcceptableAdminLevel)
        {
            return DataProvider.SetMaxAcceptableAdminLevel(applicationId, nodeTypeId, maxAcceptableAdminLevel);
        }

        public static bool set_contribution_limits(Guid applicationId, 
            Guid serviceTypeId, ref List<Guid> limitNodeTypeIds, Guid currentUserId)
        {
            return DataProvider.SetContributionLimits(applicationId, serviceTypeId, ref limitNodeTypeIds, currentUserId);
        }

        public static List<NodeType> get_contribution_limits(Guid applicationId, Guid nodeTypeIdOrNodeId)
        {
            List<NodeType> retList = new List<NodeType>();
            DataProvider.GetContributionLimits(applicationId, ref retList, nodeTypeIdOrNodeId);
            return retList;
        }

        public static bool enable_contribution(Guid applicationId, Guid nodeTypeId, bool enable)
        {
            return DataProvider.EnableContribution(applicationId, nodeTypeId, enable);
        }

        public static bool no_content_service(Guid applicationId, Guid nodeTypeIdOrNodeId, bool value)
        {
            return DataProvider.NoContentService(applicationId, nodeTypeIdOrNodeId, value);
        }

        public static bool no_content_service(Guid applicationId, Guid nodeTypeIdOrNodeId)
        {
            return DataProvider.NoContentService(applicationId, nodeTypeIdOrNodeId, null);
        }

        public static bool is_knowledge(Guid applicationId, Guid nodeTypeId, bool isKnowledge)
        {
            return DataProvider.IsKnowledge(applicationId, nodeTypeId, isKnowledge);
        }

        public static bool is_knowledge(Guid applicationId, Guid nodeTypeIdOrNodeId)
        {
            return DataProvider.IsKnowledge(applicationId, nodeTypeIdOrNodeId, null);
        }

        public static bool is_document(Guid applicationId, Guid nodeTypeId, bool isDocuemnt)
        {
            return DataProvider.IsDocument(applicationId, nodeTypeId, isDocuemnt);
        }

        public static bool is_document(Guid applicationId, Guid nodeTypeIdOrNodeId)
        {
            return DataProvider.IsDocument(applicationId, nodeTypeIdOrNodeId, null);
        }

        public static bool enable_previous_version_select(Guid applicationId, Guid nodeTypeId, bool value)
        {
            return DataProvider.EnablePreviousVersionSelect(applicationId, nodeTypeId, value);
        }

        public static bool enable_previous_version_select(Guid applicationId, Guid nodeTypeIdOrNodeId)
        {
            return DataProvider.EnablePreviousVersionSelect(applicationId, nodeTypeIdOrNodeId, null);
        }

        public static List<Guid> is_tree(Guid applicationId, List<Guid> nodeTypeOrNodeIds)
        {
            List<Guid> retList = new List<Guid>();
            DataProvider.IsTree(applicationId, ref retList, nodeTypeOrNodeIds, null);
            return retList;
        }

        public static bool is_tree(Guid applicationId, Guid nodeTypeIdOrNodeId)
        {
            List<Guid> lst = new List<Guid>();
            return is_tree(applicationId, lst).Count > 0;
        }

        public static bool is_tree(Guid applicationId, List<Guid> nodeTypeOrNodeIds, bool isTree)
        {
            List<Guid> retList = new List<Guid>();
            return DataProvider.IsTree(applicationId, ref retList, nodeTypeOrNodeIds, isTree);
        }

        public static bool is_tree(Guid applicationId, Guid nodeTypeOrNodeId, bool isTree)
        {
            List<Guid> lst = new List<Guid>();
            lst.Add(nodeTypeOrNodeId);
            List<Guid> retList = new List<Guid>();
            return DataProvider.IsTree(applicationId, ref retList, lst, isTree);
        }

        public static List<Guid> has_unique_membership(Guid applicationId, List<Guid> nodeTypeOrNodeIds)
        {
            List<Guid> retList = new List<Guid>();
            DataProvider.HasUniqueMembership(applicationId, ref retList, nodeTypeOrNodeIds, null);
            return retList;
        }

        public static bool has_unique_membership(Guid applicationId, Guid nodeTypeIdOrNodeId)
        {
            List<Guid> lst = new List<Guid>();
            return has_unique_membership(applicationId, lst).Count > 0;
        }

        public static bool has_unique_membership(Guid applicationId, List<Guid> nodeTypeOrNodeIds, bool value)
        {
            List<Guid> retList = new List<Guid>();
            return DataProvider.HasUniqueMembership(applicationId, ref retList, nodeTypeOrNodeIds, value);
        }

        public static bool has_unique_membership(Guid applicationId, Guid nodeTypeOrNodeId, bool value)
        {
            List<Guid> lst = new List<Guid>();
            lst.Add(nodeTypeOrNodeId);
            List<Guid> retList = new List<Guid>();
            return DataProvider.HasUniqueMembership(applicationId, ref retList, lst, value);
        }

        public static List<Guid> has_unique_admin_member(Guid applicationId, List<Guid> nodeTypeOrNodeIds)
        {
            List<Guid> retList = new List<Guid>();
            DataProvider.HasUniqueAdminMember(applicationId, ref retList, nodeTypeOrNodeIds, null);
            return retList;
        }

        public static bool has_unique_admin_member(Guid applicationId, Guid nodeTypeIdOrNodeId)
        {
            List<Guid> lst = new List<Guid>();
            return has_unique_admin_member(applicationId, lst).Count > 0;
        }

        public static bool has_unique_admin_member(Guid applicationId, List<Guid> nodeTypeOrNodeIds, bool value)
        {
            List<Guid> retList = new List<Guid>();
            return DataProvider.HasUniqueAdminMember(applicationId, ref retList, nodeTypeOrNodeIds, value);
        }

        public static bool has_unique_admin_member(Guid applicationId, Guid nodeTypeOrNodeId, bool value)
        {
            List<Guid> lst = new List<Guid>();
            lst.Add(nodeTypeOrNodeId);
            List<Guid> retList = new List<Guid>();
            return DataProvider.HasUniqueAdminMember(applicationId, ref retList, lst, value);
        }

        public static List<Guid> abstract_and_keywords_disabled(Guid applicationId, List<Guid> nodeTypeOrNodeIds)
        {
            List<Guid> retList = new List<Guid>();
            DataProvider.AbstractAndKeywordsDisabled(applicationId, ref retList, nodeTypeOrNodeIds, null);
            return retList;
        }

        public static bool abstract_and_keywords_disabled(Guid applicationId, Guid nodeTypeIdOrNodeId)
        {
            List<Guid> lst = new List<Guid>();
            return abstract_and_keywords_disabled(applicationId, lst).Count > 0;
        }

        public static bool abstract_and_keywords_disabled(Guid applicationId, List<Guid> nodeTypeOrNodeIds, bool value)
        {
            List<Guid> retList = new List<Guid>();
            return DataProvider.AbstractAndKeywordsDisabled(applicationId, ref retList, nodeTypeOrNodeIds, value);
        }

        public static bool abstract_and_keywords_disabled(Guid applicationId, Guid nodeTypeOrNodeId, bool value)
        {
            List<Guid> lst = new List<Guid>();
            lst.Add(nodeTypeOrNodeId);
            List<Guid> retList = new List<Guid>();
            return DataProvider.AbstractAndKeywordsDisabled(applicationId, ref retList, lst, value);
        }

        public static List<Guid> file_upload_disabled(Guid applicationId, List<Guid> nodeTypeOrNodeIds)
        {
            List<Guid> retList = new List<Guid>();
            DataProvider.FileUploadDisabled(applicationId, ref retList, nodeTypeOrNodeIds, null);
            return retList;
        }

        public static bool file_upload_disabled(Guid applicationId, Guid nodeTypeIdOrNodeId)
        {
            List<Guid> lst = new List<Guid>();
            return file_upload_disabled(applicationId, lst).Count > 0;
        }

        public static bool file_upload_disabled(Guid applicationId, List<Guid> nodeTypeOrNodeIds, bool value)
        {
            List<Guid> retList = new List<Guid>();
            return DataProvider.FileUploadDisabled(applicationId, ref retList, nodeTypeOrNodeIds, value);
        }

        public static bool file_upload_disabled(Guid applicationId, Guid nodeTypeOrNodeId, bool value)
        {
            List<Guid> lst = new List<Guid>();
            lst.Add(nodeTypeOrNodeId);
            List<Guid> retList = new List<Guid>();
            return DataProvider.FileUploadDisabled(applicationId, ref retList, lst, value);
        }

        public static List<Guid> related_nodes_select_disabled(Guid applicationId, List<Guid> nodeTypeOrNodeIds)
        {
            List<Guid> retList = new List<Guid>();
            DataProvider.RelatedNodesSelectDisabled(applicationId, ref retList, nodeTypeOrNodeIds, null);
            return retList;
        }

        public static bool related_nodes_select_disabled(Guid applicationId, Guid nodeTypeIdOrNodeId)
        {
            List<Guid> lst = new List<Guid>();
            return related_nodes_select_disabled(applicationId, lst).Count > 0;
        }

        public static bool related_nodes_select_disabled(Guid applicationId, List<Guid> nodeTypeOrNodeIds, bool value)
        {
            List<Guid> retList = new List<Guid>();
            return DataProvider.RelatedNodesSelectDisabled(applicationId, ref retList, nodeTypeOrNodeIds, value);
        }

        public static bool related_nodes_select_disabled(Guid applicationId, Guid nodeTypeOrNodeId, bool value)
        {
            List<Guid> lst = new List<Guid>();
            lst.Add(nodeTypeOrNodeId);
            List<Guid> retList = new List<Guid>();
            return DataProvider.RelatedNodesSelectDisabled(applicationId, ref retList, lst, value);
        }

        public static bool editable_for_admin(Guid applicationId, Guid nodeTypeId, bool editable)
        {
            return DataProvider.EditableForAdmin(applicationId, nodeTypeId, editable);
        }

        public static bool editable_for_creator(Guid applicationId, Guid nodeTypeId, bool editable)
        {
            return DataProvider.EditableForCreator(applicationId, nodeTypeId, editable);
        }

        public static bool editable_for_owners(Guid applicationId, Guid nodeTypeId, bool editable)
        {
            return DataProvider.EditableForOwners(applicationId, nodeTypeId, editable);
        }

        public static bool editable_for_experts(Guid applicationId, Guid nodeTypeId, bool editable)
        {
            return DataProvider.EditableForExperts(applicationId, nodeTypeId, editable);
        }

        public static bool editable_for_members(Guid applicationId, Guid nodeTypeId, bool editable)
        {
            return DataProvider.EditableForMembers(applicationId, nodeTypeId, editable);
        }

        public static bool edit_suggestion(Guid applicationId, Guid nodeTypeId, bool enable)
        {
            return DataProvider.EditSuggestion(applicationId, nodeTypeId, enable);
        }

        public static bool add_free_user(Guid applicationId, Guid nodeTypeId, Guid userId, Guid currentUserId)
        {
            return DataProvider.AddFreeUser(applicationId, nodeTypeId, userId, currentUserId);
        }

        public static bool remove_free_user(Guid applicationId, Guid nodeTypeId, Guid userId, Guid currentUserId)
        {
            return DataProvider.ArithmeticDeleteFreeUser(applicationId, nodeTypeId, userId, currentUserId);
        }

        public static List<User> get_free_users(Guid applicationId, Guid nodeTypeId)
        {
            List<User> retUsers = new List<User>();
            DataProvider.GetFreeUsers(applicationId, ref retUsers, nodeTypeId);
            return retUsers;
        }

        public static bool is_free_user(Guid applicationId, Guid nodeTypeIdOrNodeId, Guid userId)
        {
            return DataProvider.IsFreeUser(applicationId, nodeTypeIdOrNodeId, userId);
        }

        public static bool add_service_admin(Guid applicationId, Guid nodeTypeId, Guid userId, Guid currentUserId)
        {
            return DataProvider.AddServiceAdmin(applicationId, nodeTypeId, userId, currentUserId);
        }

        public static bool remove_service_admin(Guid applicationId, Guid nodeTypeId, Guid userId, Guid currentUserId)
        {
            return DataProvider.ArithmeticDeleteServiceAdmin(applicationId, nodeTypeId, userId, currentUserId);
        }

        public static List<User> get_service_admins(Guid applicationId, Guid nodeTypeId)
        {
            List<User> retUsers = new List<User>();
            DataProvider.GetServiceAdmins(applicationId, ref retUsers, nodeTypeId);
            return retUsers;
        }

        public static List<Guid> is_service_admin(Guid applicationId, List<Guid> nodeTypeIdOrNodeIds, Guid userId)
        {
            return DataProvider.IsServiceAdmin(applicationId, nodeTypeIdOrNodeIds, userId);
        }

        public static bool is_service_admin(Guid applicationId, Guid nodeTypeIdOrNodeId, Guid userId)
        {
            return DataProvider.IsServiceAdmin(applicationId, new List<Guid>() { nodeTypeIdOrNodeId }, userId).Count > 0;
        }

        public static bool register_new_node(Guid applicationId, Node nodeObject, Guid? workflowId, Guid? formInstanceId,
            Guid? wfDirectorNodeId, Guid? wfDirectorUserId, ref List<Dashboard> dashboards, ref string message)
        {
            return DataProvider.RegisterNewNode(applicationId, nodeObject, workflowId, formInstanceId,
                wfDirectorNodeId, wfDirectorUserId, ref dashboards, ref message);
        }

        public static bool register_new_node(Guid applicationId, Node nodeObject, Guid? workflowId, Guid? formInstanceId,
            Guid? wfDirectorNodeId, Guid? wfDirectorUserId, ref string message)
        {
            List<Dashboard> lst = new List<Dashboard>();
            return CNController.register_new_node(applicationId, nodeObject, workflowId, formInstanceId, 
                wfDirectorNodeId, wfDirectorUserId, ref lst, ref message);
        }

        public static bool set_admin_area(Guid applicationId, Guid nodeId, Guid? areaId)
        {
            return DataProvider.SetAdminArea(applicationId, nodeId, areaId);
        }

        public static bool set_contributors(Guid applicationId, Node info, ref string errorMessage)
        {
            return DataProvider.SetContributors(applicationId, info, ref errorMessage);
        }

        /* end of Service */
    }
}
