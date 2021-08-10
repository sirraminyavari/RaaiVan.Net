using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.GlobalUtilities.DBCompositeTypes;

namespace RaaiVan.Modules.FormGenerator
{
    public class FGController
    {
        private static string GetFullyQualifiedName(string name)
        {
            return "[dbo]." + "[FG_" + name + "]"; //'[dbo].' is database owner and 'FG_' is module qualifier
        }

        public static bool create_form(Guid applicationId, FormType info)
        {
            return DBConnector.succeed(applicationId, GetFullyQualifiedName("CreateForm"),
                applicationId, info.FormID, info.TemplateFormID, info.Title, info.Creator.UserID, DateTime.Now);
        }

        public static bool set_form_title(Guid applicationId, FormType info)
        {
            return DBConnector.succeed(applicationId, GetFullyQualifiedName("SetFormTitle"),
                applicationId, info.FormID, info.Title, info.LastModifierUserID, DateTime.Now);
        }

        public static bool set_form_name(Guid applicationId, Guid formId, string name, 
            Guid currentUserId, ref string errorMessage)
        {
            return DBConnector.succeed(applicationId, ref errorMessage, GetFullyQualifiedName("SetFormName"),
                applicationId, formId, name, currentUserId, DateTime.Now);
        }

        public static bool set_form_description(Guid applicationId, FormType info)
        {
            return DBConnector.succeed(applicationId, GetFullyQualifiedName("SetFormDescription"),
                applicationId, info.FormID, info.Description, info.LastModifierUserID, DateTime.Now);
        }

        public static bool remove_form(Guid applicationId, Guid formId, Guid currentUserId)
        {
            return DBConnector.succeed(applicationId, GetFullyQualifiedName("ArithmeticDeleteForm"),
                applicationId, formId, currentUserId, DateTime.Now);
        }

        public static bool recycle_form(Guid applicationId, Guid formId, Guid currentUserId)
        {
            return DBConnector.succeed(applicationId, GetFullyQualifiedName("RecycleForm"),
                applicationId, formId, currentUserId, DateTime.Now);
        }

        public static List<FormType> get_forms(Guid applicationId, string searchText = null, 
            int? count = null, int? lowerBoundary = null, bool? hasName = null, bool? archive = null)
        {
            return FGParsers.form_types(DBConnector.read(applicationId, GetFullyQualifiedName("GetForms"),
                applicationId, ProviderUtil.get_search_text(searchText), count, lowerBoundary, hasName, archive));
        }
        
        public static List<FormType> get_forms(Guid applicationId, List<Guid> formIds)
        {
            return FGParsers.form_types(DBConnector.read(applicationId, GetFullyQualifiedName("GetFormsByIDs"),
                applicationId, ProviderUtil.list_to_string<Guid>(formIds), ','));
        }

        public static FormType get_form(Guid applicationId, Guid formId)
        {
            return get_forms(applicationId, new List<Guid>() { formId }).FirstOrDefault();
        }

        public static bool add_form_element(Guid applicationId, FormElement info, ref string errorMessage)
        {
            if (string.IsNullOrEmpty(info.Name)) info.Name = null;
            FormElementTypes type = info.Type.HasValue ? info.Type.Value : FormElementTypes.Text;

            return DBConnector.succeed(applicationId, ref errorMessage, GetFullyQualifiedName("AddFormElement"),
                applicationId, info.ElementID, info.FormID, info.Title, info.Name, info.Help, info.SequenceNumber,
                type.ToString(), info.Info, info.Creator.UserID, DateTime.Now);
        }

        public static bool modify_form_element(Guid applicationId, FormElement info, ref string errorMessage)
        {
            return DBConnector.succeed(applicationId, ref errorMessage, GetFullyQualifiedName("ModifyFormElement"),
                applicationId, info.ElementID, info.Title, info.Name, info.Help,
                info.Info, info.Weight, info.LastModifierUserID, DateTime.Now);
        }

        public static bool set_elements_order(Guid applicationId, List<Guid> elementIds)
        {
            return DBConnector.succeed(applicationId, GetFullyQualifiedName("SetElementsOrder"),
                applicationId, ProviderUtil.list_to_string<Guid>(elementIds), ',');
        }

        public static bool set_form_element_necessity(Guid applicationId, Guid elementId, bool necessity)
        {
            return DBConnector.succeed(applicationId, GetFullyQualifiedName("SetFormElementNecessity"),
                applicationId, elementId, necessity);
        }

        public static bool set_form_element_uniqueness(Guid applicationId, Guid elementId, bool value)
        {
            return DBConnector.succeed(applicationId, GetFullyQualifiedName("SetFormElementUniqueness"), 
                applicationId, elementId, value);
        }

        public static bool remove_form_element(Guid applicationId, Guid elementId, Guid currentUserId)
        {
            return DBConnector.succeed(applicationId, GetFullyQualifiedName("ArithmeticDeleteFormElement"),
                applicationId, elementId, currentUserId, DateTime.Now);
        }

        public static bool save_form_elements(Guid applicationId, Guid formId,
            string title, string name, string description, List<FormElement> elements, Guid currentUserId)
        {
            if (elements == null) elements = new List<FormElement>();

            int seq = 1;

            DBCompositeType<FormElementTableType> elementsParam = new DBCompositeType<FormElementTableType>()
                .add(elements.Select(e =>
                {
                    e.SequenceNumber = seq++;

                    string strType = null;
                    if (e.Type.HasValue) strType = e.Type.Value.ToString();

                    return new FormElementTableType(
                        elementId: e.ElementID,
                        templateElementId: e.TemplateElementID,
                        instanceId: Guid.NewGuid(),
                        refElementId: e.RefElementID,
                        title: PublicMethods.verify_string(e.Title),
                        name: e.Name,
                        sequenceNumber: e.SequenceNumber,
                        necessary: e.Necessary,
                        uniqueValue: e.UniqueValue,
                        type: strType,
                        help: PublicMethods.verify_string(e.Help),
                        info: e.Info,
                        weight: e.Weight,
                        textValue: PublicMethods.verify_string(e.TextValue),
                        floatValue: e.FloatValue,
                        bitValue: e.BitValue,
                        dateValue: e.DateValue);
                }).ToList());

            return DBConnector.succeed(applicationId, GetFullyQualifiedName("SaveFormElements"),
                applicationId, formId, title, name, description, elementsParam, currentUserId, DateTime.Now);
        }

        public static List<FormElement> get_form_elements(Guid applicationId, 
            Guid? formId, Guid? ownerId = null, FormElementTypes? type = null)
        {
            string strType = null;
            if (type.HasValue) strType = type.Value.ToString();

            return FGParsers.form_elements(DBConnector.read(applicationId, GetFullyQualifiedName("GetFormElements"),
                applicationId, formId, ownerId, strType));
        }

        public static List<FormElement> get_form_elements(Guid applicationId, List<Guid> elementIds)
        {
            return FGParsers.form_elements(DBConnector.read(applicationId, GetFullyQualifiedName("GetFormElementsByIDs"),
                applicationId, string.Join(",", elementIds), ','));
        }

        public static FormElement get_form_element(Guid applicationId, Guid elementId)
        {
            return get_form_elements(applicationId, new List<Guid>() { elementId }).FirstOrDefault();
        }

        public static Dictionary<string, Guid> get_form_element_ids(Guid applicationId, Guid formId, List<string> names)
        {
            return FGParsers.element_ids(DBConnector.read(applicationId, GetFullyQualifiedName("GetFormElementIDs"),
                applicationId, formId, string.Join(",", names), ','));
        }

        public static List<Guid> is_form_element(Guid applicationId, List<Guid> ids)
        {
            return DBConnector.get_guid_list(applicationId, GetFullyQualifiedName("IsFormElement"),
                applicationId, ProviderUtil.list_to_string<Guid>(ids), ',');
        }

        public static bool is_form_element(Guid applicationId, Guid id)
        {
            return is_form_element(applicationId, new List<Guid>() { id }).Count > 0;
        }

        public static bool create_form_instances(Guid applicationId, List<FormType> instances, Guid currentUserId)
        {
            if (instances == null) instances = new List<FormType>();

            DBCompositeType<FormInstanceTableType> instancesParam = new DBCompositeType<FormInstanceTableType>()
                .add(instances.Select(i => new FormInstanceTableType(
                    instanceId: i.InstanceID,
                    formId: i.FormID,
                    ownerId: i.OwnerID,
                    directorId: i.DirectorID,
                    admin: i.Admin,
                    isTemporary: i.IsTemporary
                    )).ToList());

            return DBConnector.succeed(applicationId, GetFullyQualifiedName("CreateFormInstance"),
                applicationId, instancesParam, currentUserId, DateTime.Now);
        }

        public static bool create_form_instance(Guid applicationId, FormType info)
        {
            return info.Creator.UserID.HasValue &&
                create_form_instances(applicationId, new List<FormType>() { info }, info.Creator.UserID.Value);
        }

        public static bool remove_form_instances(Guid applicationId, List<Guid> instanceIds, Guid currentUserId)
        {
            return DBConnector.succeed(applicationId, GetFullyQualifiedName("RemoveFormInstances"),
                applicationId, ProviderUtil.list_to_string<Guid>(instanceIds), ',', currentUserId, DateTime.Now);
        }

        public static bool remove_form_instance(Guid applicationId, Guid instanceId, Guid currentUserId)
        {
            return remove_form_instances(applicationId, new List<Guid>() { instanceId }, currentUserId);
        }

        public static bool remove_owner_form_instances(Guid applicationId, Guid ownerId, Guid formId, Guid currentUserId)
        {
            return DBConnector.succeed(applicationId, GetFullyQualifiedName("RemoveOwnerFormInstances"),
                applicationId, ownerId, formId, currentUserId, DateTime.Now);
        }

        public static List<FormType> get_owner_form_instances(Guid applicationId, 
            List<Guid> ownerIds, Guid? formId = null, bool? isTemporary = null, Guid? userId = null)
        {
            List<FormType> retList = new List<FormType>();

            PublicMethods.split_list<Guid>(ownerIds, 200, ids =>
            {
                List<FormType> newList = FGParsers.form_instances(
                    DBConnector.read(applicationId, GetFullyQualifiedName("GetOwnerFormInstances"),
                    applicationId, ProviderUtil.list_to_string<Guid>(ownerIds), ',', formId, isTemporary, userId));

                if (newList.Count > 0) retList.AddRange(newList);
            });
            
            return retList;
        }

        public static List<FormType> get_owner_form_instances(Guid applicationId, Guid ownerId, 
            Guid? formId = null, bool? isTemporary = null, Guid? userId = null)
        {
            return get_owner_form_instances(applicationId, new List<Guid>() { ownerId }, formId, isTemporary, userId);
        }

        public static List<FormType> get_form_instances(Guid applicationId, List<Guid> instanceIds)
        {
            return FGParsers.form_instances(DBConnector.read(applicationId, GetFullyQualifiedName("GetFormInstances"),
                applicationId, ProviderUtil.list_to_string<Guid>( instanceIds), ','));
        }

        public static FormType get_form_instance(Guid applicationId, Guid instanceId)
        {
            return get_form_instances(applicationId, new List<Guid>() { instanceId }).FirstOrDefault();
        }

        public static Guid? get_form_instance_owner_id(Guid applicationId, Guid instanceIdOrElementId)
        {
            return DBConnector.get_guid(applicationId, GetFullyQualifiedName("GetFormInstanceOwnerID"),
                applicationId, instanceIdOrElementId);
        }

        public static Guid? get_form_instance_hierarchy_owner_id(Guid applicationId, Guid instanceId)
        {
            return DBConnector.get_guid(applicationId, GetFullyQualifiedName("GetFormInstanceHierarchyOwnerID"),
                applicationId, instanceId);
        }

        public static bool validate_new_name(Guid applicationId, Guid objectId, Guid? formId, string name)
        {
            if (!string.IsNullOrEmpty(name)) name = name.Trim().ToLower();

            return string.IsNullOrEmpty(name) || (FGUtilities.is_valid_name(name) && 
                DBConnector.succeed(applicationId, GetFullyQualifiedName("ValidateNewName"), applicationId, objectId, formId, name));
        }

        public static bool meets_unique_constraint(Guid applicationId,
            Guid instanceId, Guid elementId, string textValue, double? floatValue)
        {
            return DBConnector.succeed(applicationId, GetFullyQualifiedName("MeetsUniqueConstraint"),
                applicationId, instanceId, elementId, textValue, floatValue);
        }

        public static bool save_form_instance_elements(Guid applicationId, 
            List<FormElement> elements, List<Guid> elementsToClear, Guid currentUserId, ref string errorMessage)
        {
            if (elements == null) elements = new List<FormElement>();
            if (elementsToClear == null) elementsToClear = new List<Guid>();

            if (elements.Any(u => !string.IsNullOrEmpty(u.Info) && u.Info.Length > 3900))
            {
                errorMessage = Messages.MaxAllowedInputLengthExceeded.ToString();
                return false;
            }

            DBCompositeType<FormElementTableType> elementsParam = new DBCompositeType<FormElementTableType>()
                .add(elements.Select(e =>
                {
                    return new FormElementTableType(
                        elementId: e.ElementID,
                        templateElementId: e.TemplateElementID,
                        instanceId: e.FormInstanceID,
                        refElementId: e.RefElementID,
                        title: PublicMethods.verify_string(e.Title),
                        name: e.Name,
                        sequenceNumber: e.SequenceNumber,
                        necessary: e.Necessary,
                        uniqueValue: e.UniqueValue,
                        type: !e.Type.HasValue ? null : e.Type.Value.ToString(),
                        help: PublicMethods.verify_string(e.Help),
                        info: e.Info,
                        weight: e.Weight,
                        textValue: PublicMethods.verify_string(e.TextValue),
                        floatValue: e.FloatValue,
                        bitValue: e.BitValue,
                        dateValue: e.DateValue);
                }).ToList());

            //Guid Items Param
            List<FormElementTypes> validTypes =
                new List<FormElementTypes>() { FormElementTypes.Node, FormElementTypes.User, FormElementTypes.MultiLevel };

            DBCompositeType<GuidPairTableType> guidItemsParam = new DBCompositeType<GuidPairTableType>();

            elements.Where(u => validTypes.Any(v => v == u.Type)).ToList()
                .ForEach(u => u.GuidItems.Where(i => i.ID.HasValue).ToList()
                .ForEach(x => guidItemsParam.add(new GuidPairTableType(u.ElementID.Value, x.ID.Value))));
            //end of Guid Items Param

            DBCompositeType<GuidTableType> elementsToClearParam = new DBCompositeType<GuidTableType>()
                .add(elementsToClear.Select(e => new GuidTableType(e)).ToList());

            //Files Param
            DBCompositeType<DocFileInfoTableType> filesParam = new DBCompositeType<DocFileInfoTableType>();

            elements.Where(e => e.AttachedFiles != null).ToList().ForEach(e => {
                e.AttachedFiles.ForEach(f =>
                {
                    filesParam.add(new DocFileInfoTableType(
                        fileId: f.FileID,
                        fileName: f.FileName,
                        extension: f.Extension,
                        mime: f.MIME(),
                        size: f.Size,
                        ownerId: f.OwnerID,
                        ownerType: f.OwnerType.ToString()));
                });
            });
            //end of Files Param

            return DBConnector.succeed(applicationId, GetFullyQualifiedName("SaveFormInstanceElements"),
                applicationId, elementsParam, guidItemsParam, elementsToClearParam, filesParam, currentUserId, DateTime.Now);
        }

        public static bool save_form_instance_elements(Guid applicationId,
            List<FormElement> elements, List<Guid> elementsToClear, Guid currentUserId)
        {
            string errorMessage = string.Empty;
            return save_form_instance_elements(applicationId, elements, elementsToClear, currentUserId, ref errorMessage);
        }

        public static bool save_form_instance_element(Guid applicationId, FormElement element, Guid currentUserId)
        {
            return save_form_instance_elements(applicationId, new List<FormElement>() { element }, new List<Guid>(), currentUserId);
        }

        public static List<FormElement> get_form_instance_elements(Guid applicationId, 
            List<Guid> instanceIds, List<Guid> elementIds, bool? filled = null)
        {
            List<FormElement> retList = new List<FormElement>();

            PublicMethods.split_list<Guid>(instanceIds, 200, ids =>
            {
                List<FormElement> newList = 
                    FGParsers.form_instance_elements(DBConnector.read(applicationId, GetFullyQualifiedName("GetFormInstanceElements"),
                    applicationId, string.Join(",", instanceIds), filled, string.Join(",", elementIds), ','));

                if (newList.Count > 0) retList.AddRange(newList);
            });
            
            return retList;
        }

        public static List<FormElement> get_form_instance_elements(Guid applicationId,
            Guid instanceId, List<Guid> elementIds, bool? filled = null)
        {
            return get_form_instance_elements(applicationId, new List<Guid>() { instanceId }, elementIds, filled);
        }

        public static List<FormElement> get_form_instance_elements(Guid applicationId, 
            List<Guid> instanceIds, bool? filled = null)
        {
            return get_form_instance_elements(applicationId, instanceIds, new List<Guid>(), filled);
        }

        public static List<FormElement> get_form_instance_elements(Guid applicationId,
            Guid instanceId, bool? filled = null)
        {
            return get_form_instance_elements(applicationId, new List<Guid>() { instanceId }, new List<Guid>(), filled);
        }

        public static Dictionary<Guid, List<SelectedGuidItem>> get_selected_guids(Guid applicationId, List<Guid> elementIds)
        {
            if (elementIds == null || elementIds.Count == 0) return new Dictionary<Guid, List<SelectedGuidItem>>();

            return FGParsers.selected_guids(DBConnector.read(applicationId, GetFullyQualifiedName("GetSelectedGuids"),
                applicationId, ProviderUtil.list_to_string<Guid>(elementIds), ','));
        }

        public static List<FormElement> get_element_changes(Guid applicationId, Guid elementId, int? count, int? lowerBoundary)
        {
            return FGParsers.element_changes(DBConnector.read(applicationId, GetFullyQualifiedName("GetElementChanges"),
                applicationId, elementId, count, lowerBoundary));
        }

        public static bool set_form_instance_as_filled(Guid applicationId, Guid instanceId, Guid currentUserId)
        {
            return DBConnector.succeed(applicationId, GetFullyQualifiedName("SetFormInstanceAsFilled"),
                applicationId, instanceId, DateTime.Now, currentUserId);
        }

        public static bool set_form_instance_as_not_filled(Guid applicationId, Guid instanceId, Guid currentUserId)
        {
            return DBConnector.succeed(applicationId, GetFullyQualifiedName("SetFormInstanceAsNotFilled"),
                applicationId, instanceId, currentUserId);
        }

        public static bool is_director(Guid applicationId, Guid instanceId, Guid userId)
        {
            return DBConnector.succeed(applicationId, GetFullyQualifiedName("IsDirector"), applicationId, instanceId, userId);
        }

        public static bool set_form_owner(Guid applicationId, Guid ownerId, Guid formId, Guid currentUserId)
        {
            return DBConnector.succeed(applicationId, GetFullyQualifiedName("SetFormOwner"),
                applicationId, ownerId, formId, currentUserId, DateTime.Now);
        }

        public static bool remove_form_owner(Guid applicationId, Guid ownerId, Guid formId, Guid currentUserId)
        {
            return DBConnector.succeed(applicationId, GetFullyQualifiedName("ArithmeticDeleteFormOwner"),
                applicationId, ownerId, formId, currentUserId, DateTime.Now);
        }

        public static FormType get_owner_form(Guid applicationId, Guid ownerId)
        {
            return FGParsers.form_types(DBConnector.read(applicationId, GetFullyQualifiedName("GetOwnerForm"),
                 applicationId, ownerId)).FirstOrDefault();
        }

        public static Guid? initialize_owner_form_instance(Guid applicationId, Guid ownerId, Guid? formId, Guid currentUserId)
        {
            return DBConnector.get_guid(applicationId, GetFullyQualifiedName("InitializeOwnerFormInstance"),
                applicationId, ownerId, formId, currentUserId, DateTime.Now);
        }

        public static bool set_element_limits(Guid applicationId, Guid ownerId, List<Guid> elementIds, Guid currentUserId)
        {
            return DBConnector.succeed(applicationId, GetFullyQualifiedName("SetElementLimits"),
                applicationId, ownerId, ProviderUtil.list_to_string<Guid>(elementIds), ',', currentUserId, DateTime.Now);
        }

        public static List<FormElement> get_element_limits(Guid applicationId, Guid ownerId)
        {
            return FGParsers.element_limits(DBConnector.read(applicationId, GetFullyQualifiedName("GetElementLimits"),
                applicationId, ownerId));
        }

        public static bool set_element_limit_necessity(Guid applicationId, 
            Guid ownerId, Guid elementId, bool necessary, Guid currentUserId)
        {
            return DBConnector.succeed(applicationId, GetFullyQualifiedName("SetElementLimitNecessity"),
                applicationId, ownerId, elementId, necessary, currentUserId, DateTime.Now);
        }

        public static bool remove_element_limit(Guid applicationId, Guid ownerId, Guid elementId, Guid currentUserId)
        {
            return DBConnector.succeed(applicationId, GetFullyQualifiedName("ArithmeticDeleteElementLimit"),
                applicationId, ownerId, elementId, currentUserId, DateTime.Now);
        }

        public static List<Guid> get_common_form_instance_ids(Guid applicationId, 
            Guid ownerId, Guid filledOwnerId, bool hasLimit)
        {
            return DBConnector.get_guid_list(applicationId, GetFullyQualifiedName("GetCommonFormInstanceIDs"),
                applicationId, ownerId, filledOwnerId, hasLimit);
        }

        public static List<FormRecord> get_form_records(Guid applicationId, Guid formId,
            List<Guid> elementIds, List<Guid> instanceIds, List<Guid> ownerIds, List<FormFilter> filters,
            int? lowerBoundary, int? count, Guid? sortByElementId, bool? descending)
        {
            //prepare
            if (elementIds == null) elementIds = new List<Guid>();
            if (instanceIds == null) instanceIds = new List<Guid>();
            if (ownerIds == null) ownerIds = new List<Guid>();
            if (filters == null) filters = new List<FormFilter>();

            List<FormElement> elements = get_form_elements(applicationId, formId);

            if (elementIds != null && elementIds.Count > 0)
            {
                elementIds = elementIds.Where(u => elements.Any(v => v.ElementID == u)).ToList();

                elements = elements.Where(u => elementIds.Any(v => v == u.ElementID))
                    .OrderBy(x => x.SequenceNumber).ToList();
            }
            //end of prepare

            DBCompositeType<GuidTableType> elementIdsParam = new DBCompositeType<GuidTableType>()
                .add(elementIds.Select(e => new GuidTableType(e)).ToList());

            DBCompositeType<GuidTableType> instanceIdsParam = new DBCompositeType<GuidTableType>()
                .add(instanceIds.Select(e => new GuidTableType(e)).ToList());

            DBCompositeType<GuidTableType> ownerIdsParam = new DBCompositeType<GuidTableType>()
                .add(ownerIds.Select(e => new GuidTableType(e)).ToList());

            DBCompositeType<FormFilterTableType> filtersParam = new DBCompositeType<FormFilterTableType>()
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

            DBResultSet results = DBConnector.read(applicationId, GetFullyQualifiedName("GetFormRecords"),
                applicationId, formId, elementIdsParam, instanceIdsParam, ownerIdsParam, filtersParam,
                lowerBoundary, count, sortByElementId, descending);

            return FGParsers.form_records(results, elements);
        }

        public static List<FormRecord> get_form_records(Guid applicationId, Guid formId, List<FormFilter> filters, 
            int? lowerBoundary = null, int? count = null, Guid? sortByElementId = null, bool? descending = false)
        {
            return get_form_records(applicationId, formId, new List<Guid>(), new List<Guid>(), new List<Guid>(),
                filters, lowerBoundary, count, sortByElementId, descending);
        }

        public static List<FormRecord> get_form_records(Guid applicationId, Guid formId,
            int? lowerBoundary = null, int? count = 20, Guid? sortByElementId = null, bool? descending = false)
        {
            return get_form_records(applicationId, formId, new List<Guid>(), new List<Guid>(), new List<Guid>(),
                new List<FormFilter>(), lowerBoundary, count, sortByElementId, descending);
        }

        public static void get_form_statistics(Guid applicationId, Guid? ownerId, Guid? instanceId,
            ref double weightSum, ref double sum, ref double weightedSum, ref double avg, ref double weightedAvg,
            ref double min, ref double max, ref double var, ref double stDev)
        {
            DBResultSet results = DBConnector.read(applicationId, GetFullyQualifiedName("GetFormStatistics"),
                applicationId, ownerId, instanceId);

            FGParsers.form_statistics(results, ref weightSum, ref sum, ref weightedSum, ref avg,
                ref weightedAvg, ref min, ref max, ref var, ref stDev);
        }

        public static bool convert_form_to_table(Guid applicationId, Guid formId)
        {
            return DBConnector.succeed(applicationId, GetFullyQualifiedName("ConvertFormToTable"), applicationId, formId);
        }

        public static List<TemplateStatus> get_template_status(Guid applicationId, List<Guid> templateIds)
        {
            return FGParsers.template_status(DBConnector.read(applicationId, GetFullyQualifiedName("GetTemplateStatus"),
                applicationId, RaaiVanSettings.ReferenceTenantID, string.Join(",", templateIds.Select(id => id.ToString())), ','));
        }

        //Polls

        public static List<Poll> get_polls(Guid applicationId, Guid? isCopyOfPollId, Guid? ownerId,
            bool? archive, string searchText, int? count, long? lowerBoundary, ref long totalCount)
        {
            DBResultSet results = DBConnector.read(applicationId, GetFullyQualifiedName("GetPolls"),
                applicationId, isCopyOfPollId, ownerId, archive, ProviderUtil.get_search_text(searchText), count, lowerBoundary);

            return FGParsers.polls(results, ref totalCount);
        }

        public static List<Poll> get_polls(Guid applicationId, List<Guid> pollIds)
        {
            return FGParsers.polls(DBConnector.read(applicationId, GetFullyQualifiedName("GetPollsByIDs"),
                applicationId, string.Join(",", pollIds), ','));
        }

        public static Poll get_poll(Guid applicationId, Guid pollId)
        {
            return get_polls(applicationId, new List<Guid>() { pollId }).FirstOrDefault();
        }

        public static bool add_poll(Guid applicationId, 
            Guid pollId, Guid? copyFromPollId, Guid? ownerId, string name, Guid currentUserId)
        {
            if (string.IsNullOrEmpty(name)) name = null;

            return DBConnector.succeed(applicationId, GetFullyQualifiedName("AddPoll"),
                applicationId, pollId, copyFromPollId, ownerId, name, currentUserId, DateTime.Now);
        }

        public static Guid? get_poll_instance(Guid applicationId,
            Guid? pollId, Guid copyFromPollId, Guid? ownerId, Guid currentUserId)
        {
            return DBConnector.get_guid(applicationId, GetFullyQualifiedName("GetPollInstance"),
                applicationId, pollId, copyFromPollId, ownerId, currentUserId, DateTime.Now);
        }

        public static List<Guid> get_owner_poll_ids(Guid applicationId, Guid isCopyOfPollId, Guid ownerId)
        {
            return DBConnector.get_guid_list(applicationId, GetFullyQualifiedName("GetOwnerPollIDs"),
                applicationId, isCopyOfPollId, ownerId);
        }

        public static bool rename_poll(Guid applicationId, Guid pollId, string name, Guid currentUserId)
        {
            return DBConnector.succeed(applicationId, GetFullyQualifiedName("RenamePoll"),
                applicationId, pollId, name, currentUserId, DateTime.Now);
        }

        public static bool set_poll_description(Guid applicationId, Guid pollId, string description, Guid currentUserId)
        {
            if (string.IsNullOrEmpty(description)) description = null;

            return DBConnector.succeed(applicationId, GetFullyQualifiedName("SetPollDescription"),
                applicationId, pollId, description, currentUserId, DateTime.Now);
        }

        public static bool set_poll_begin_date(Guid applicationId, Guid pollId, DateTime? beginDate, Guid currentUserId)
        {
            return DBConnector.succeed(applicationId, GetFullyQualifiedName("SetPollBeginDate"),
                applicationId, pollId, beginDate, currentUserId, DateTime.Now);
        }

        public static bool set_poll_finish_date(Guid applicationId, Guid pollId, DateTime? finishDate, Guid currentUserId)
        {
            return DBConnector.succeed(applicationId, GetFullyQualifiedName("SetPollFinishDate"),
                applicationId, pollId, finishDate, currentUserId, DateTime.Now);
        }

        public static bool set_poll_show_summary(Guid applicationId, Guid pollId, bool showSummary, Guid currentUserId)
        {
            return DBConnector.succeed(applicationId, GetFullyQualifiedName("SetPollShowSummary"),
                applicationId, pollId, showSummary, currentUserId, DateTime.Now);
        }

        public static bool set_poll_hide_contributors(Guid applicationId, 
            Guid pollId, bool hideContributors, Guid currentUserId)
        {
            return DBConnector.succeed(applicationId, GetFullyQualifiedName("SetPollHideContributors"),
                applicationId, pollId, hideContributors, currentUserId, DateTime.Now);
        }

        public static bool remove_poll(Guid applicationId, Guid pollId, Guid currentUserId)
        {
            return DBConnector.succeed(applicationId, GetFullyQualifiedName("RemovePoll"),
                applicationId, pollId, currentUserId, DateTime.Now);
        }

        public static bool recycle_poll(Guid applicationId, Guid pollId, Guid currentUserId)
        {
            return DBConnector.succeed(applicationId, GetFullyQualifiedName("RecyclePoll"),
                applicationId, pollId, currentUserId, DateTime.Now);
        }

        public static void get_poll_status(Guid applicationId, Guid? pollId, Guid? isCopyOfPollId, 
            Guid currentUserId, ref string description, ref DateTime? beginDate, ref DateTime? finishDate, 
            ref Guid? instanceId, ref int? elementsCount, ref int? filledElementsCount, ref int? allFilledFormsCount)
        {
            DBResultSet results = DBConnector.read(applicationId, GetFullyQualifiedName("GetPollStatus"),
                applicationId, pollId, isCopyOfPollId, currentUserId);

            FGParsers.poll_status(results, ref description, ref beginDate, ref finishDate, ref instanceId,
                    ref elementsCount, ref filledElementsCount, ref allFilledFormsCount);
        }

        public static Dictionary<Guid, int> get_poll_elements_instance_count(Guid applicationId, Guid pollId)
        {
            return DBConnector.get_items_count(applicationId, GetFullyQualifiedName("GetPollElementsInstanceCount"),
                applicationId, pollId);
        }

        public static List<PollAbstract> get_poll_abstract_text(Guid applicationId,
            Guid pollId, List<Guid> elementIds, int? count, int? lowerBoundary)
        {
            return FGParsers.poll_abstract(DBConnector.read(applicationId, GetFullyQualifiedName("GetPollAbstractText"),
                applicationId, pollId, string.Join(",", elementIds), ',', count, lowerBoundary));
        }

        public static List<PollAbstract> get_poll_abstract_guid(Guid applicationId, 
            Guid pollId, List<Guid> elementIds, int? count, int? lowerBoundary)
        {
            return FGParsers.poll_abstract(DBConnector.read(applicationId, GetFullyQualifiedName("GetPollAbstractGuid"),
                applicationId, pollId, string.Join(",", elementIds), ',', count, lowerBoundary));
        }

        public static List<PollAbstract> get_poll_abstract_bool(Guid applicationId, Guid pollId, List<Guid> elementIds)
        {
            return FGParsers.poll_abstract(DBConnector.read(applicationId, GetFullyQualifiedName("GetPollAbstractBool"),
                applicationId, pollId, string.Join(",", elementIds), ','));
        }

        public static List<PollAbstract> get_poll_abstract_number(Guid applicationId, 
            Guid pollId, List<Guid> elementIds, int? count, int? lowerBoundary)
        {
            return FGParsers.poll_abstract(DBConnector.read(applicationId, GetFullyQualifiedName("GetPollAbstractNumber"),
                applicationId, pollId, string.Join(",", elementIds), ',', count, lowerBoundary));
        }

        public static List<FormElement> get_poll_element_instances(Guid applicationId, 
            Guid pollId, Guid elementId, int? count, int? lowerBoundary)
        {
            return FGParsers.poll_element_instances(DBConnector.read(applicationId, GetFullyQualifiedName("GetPollElementInstances"),
                applicationId, pollId, elementId, count, lowerBoundary));
        }

        public static void get_current_polls_count(Guid applicationId, Guid? currentUserId, ref int count, ref int doneCount)
        {
            DBResultSet results = DBConnector.read(applicationId, GetFullyQualifiedName("GetCurrentPollsCount"),
                applicationId, currentUserId, DateTime.Now, RaaiVanSettings.DefaultPrivacy(applicationId));

            FGParsers.current_polls_count(results, ref count, ref doneCount);
        }

        public static Dictionary<Guid, bool> get_current_polls(Guid applicationId, 
            Guid? currentUserId, int? count, int? lowerBoundary, ref long totalCount)
        {
            return DBConnector.get_items_status_bool(applicationId, ref totalCount, GetFullyQualifiedName("GetCurrentPolls"), 
                applicationId, currentUserId, DateTime.Now, RaaiVanSettings.DefaultPrivacy(applicationId), count, lowerBoundary);
        }

        public static List<Guid> is_poll(Guid applicationId, List<Guid> ids)
        {
            return DBConnector.get_guid_list(applicationId, GetFullyQualifiedName("IsPoll"),
                applicationId, ProviderUtil.list_to_string<Guid>(ids), ',');
        }

        public static bool is_poll(Guid applicationId, Guid id)
        {
            return is_poll(applicationId, new List<Guid>() { id }).Count > 0;
        }

        //end of Polls
    }
}
    