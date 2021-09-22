using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using System.Data;
using RaaiVan.Modules.WorkFlow;
using RaaiVan.Modules.Documents;
using RaaiVan.Modules.CoreNetwork;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.FormGenerator;
using RaaiVan.Modules.Users;
using RaaiVan.Modules.Sharing;
using RaaiVan.Modules.NotificationCenter;
using RaaiVan.Modules.Log;
using RaaiVan.Modules.Messaging;

namespace RaaiVan.Web.API
{
    /// <summary>
    /// Summary description for WFAPI
    /// </summary>
    public class WFAPI : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public ParamsContainer paramsContainer = null;

        public void ProcessRequest(HttpContext context)
        {
            paramsContainer = new ParamsContainer(context, nullTenantResponse: true);
            if (!paramsContainer.ApplicationID.HasValue) return;

            string responseText = string.Empty;
            string command = PublicMethods.parse_string(context.Request.Params["Command"], false);

            Guid userId = string.IsNullOrEmpty(context.Request.Params["UserID"]) ? Guid.Empty : Guid.Parse(context.Request.Params["UserID"]);
            Guid formId = string.IsNullOrEmpty(context.Request.Params["FormID"]) ? Guid.Empty : Guid.Parse(context.Request.Params["FormID"]);
            Guid workFlowId = string.IsNullOrEmpty(context.Request.Params["WorkFlowID"]) ? Guid.Empty : Guid.Parse(context.Request.Params["WorkFlowID"]);
            Guid stateId = string.IsNullOrEmpty(context.Request.Params["StateID"]) ? Guid.Empty : Guid.Parse(context.Request.Params["StateID"]);
            Guid inStateId = string.IsNullOrEmpty(context.Request.Params["InStateID"]) ? Guid.Empty : Guid.Parse(context.Request.Params["InStateID"]);
            Guid outStateId = string.IsNullOrEmpty(context.Request.Params["OutStateID"]) ? Guid.Empty : Guid.Parse(context.Request.Params["OutStateID"]);
            Guid refStateId = string.IsNullOrEmpty(context.Request.Params["RefStateID"]) ? Guid.Empty : Guid.Parse(context.Request.Params["RefStateID"]);
            Guid nodeId = string.IsNullOrEmpty(context.Request.Params["NodeID"]) ? Guid.Empty : Guid.Parse(context.Request.Params["NodeID"]);
            Guid nodeTypeId = string.IsNullOrEmpty(context.Request.Params["NodeTypeID"]) ? Guid.Empty : Guid.Parse(context.Request.Params["NodeTypeID"]);
            Guid ownerId = string.IsNullOrEmpty(context.Request.Params["OwnerID"]) ? Guid.Empty : Guid.Parse(context.Request.Params["OwnerID"]);
            Guid historyId = string.IsNullOrEmpty(context.Request.Params["HistoryID"]) ? Guid.Empty : Guid.Parse(context.Request.Params["HistoryID"]);
            
            List<DocFileInfo> attachedFiles = string.IsNullOrEmpty(context.Request.Params["AttachedFiles"]) ? new List<DocFileInfo>() :
                DocumentUtilities.get_files_info(paramsContainer.ApplicationID, context.Request.Params["AttachedFiles"]);

            string strAttachmentRequired = string.IsNullOrEmpty(context.Request.Params["AttachmentRequired"]) ?
                string.Empty : context.Request.Params["AttachmentRequired"];
            bool attachmentRequired = strAttachmentRequired.ToLower() == "true" ? true : false;
            string strNodeRequired = string.IsNullOrEmpty(context.Request.Params["NodeRequired"]) ? 
                string.Empty : context.Request.Params["NodeRequired"];
            bool nodeRequired = strNodeRequired.ToLower() == "true" ? true : false;

            string strNecessary = string.IsNullOrEmpty(context.Request.Params["Necessary"]) ? 
                string.Empty : context.Request.Params["Necessary"];
            bool necessary = strNecessary.ToLower() == "true";

            string name = string.IsNullOrEmpty(context.Request.Params["Name"]) ? string.Empty : Base64.decode(context.Request.Params["Name"]);
            string label = string.IsNullOrEmpty(context.Request.Params["Label"]) ? string.Empty : Base64.decode(context.Request.Params["Label"]);
            string title = string.IsNullOrEmpty(context.Request.Params["Title"]) ? string.Empty : Base64.decode(context.Request.Params["Title"]);
            string description = string.IsNullOrEmpty(context.Request.Params["Description"]) ? string.Empty : Base64.decode(context.Request.Params["Description"]);
            string attachmentTitle = string.IsNullOrEmpty(context.Request.Params["AttachmentTitle"]) ? string.Empty :
                Base64.decode(context.Request.Params["AttachmentTitle"]);
            
            switch (command)
            {
                case "CreateState":
                    create_state(title, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "ModifyState":
                    modify_state(stateId, title, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "RemoveState":
                    remove_state(stateId, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "GetStates":
                    get_states(workFlowId, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "CreateWorkFlow":
                    create_workflow(name, description, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "ModifyWorkFlow":
                    modify_workflow(workFlowId, name, description, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "RemoveWorkFlow":
                    remove_workflow(workFlowId, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "GetWorkFlows":
                    get_workflows(ref responseText);
                    _return_response(ref responseText);
                    return;
                case "GetWorkFlow":
                    get_workflow(workFlowId, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "AddWorkFlowState":
                    add_workflow_state(workFlowId, stateId, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "RemoveWorkFlowState":
                    remove_workflow_state(workFlowId, stateId, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "SetWorkFlowStateDescription":
                    set_workflow_state_description(workFlowId, stateId, description, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "SetWorkFlowStateTag":
                    string tag = string.IsNullOrEmpty(context.Request.Params["Tag"]) ? string.Empty : Base64.decode(context.Request.Params["Tag"]);
                    set_workflow_state_tag(workFlowId, stateId, tag, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "RemoveWorkFlowStateTag":
                    remove_workflow_state_tag(workFlowId, stateId, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "GetWorkFlowTags":
                    get_workflow_tags(workFlowId, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "SetStateDirector":
                    {
                        StateResponseTypes responseType = PublicMethods.parse_enum<StateResponseTypes>(
                            context.Request.Params["ResponseType"], defaultValue: StateResponseTypes.None);

                        bool admin = PublicMethods.parse_bool(context.Request.Params["isAdmin"],
                            defaultValue: PublicMethods.parse_bool(context.Request.Params["Admin"], defaultValue: false)).Value;

                        set_state_director(PublicMethods.parse_guid(context.Request.Params["WorkFlowID"]),
                            PublicMethods.parse_guid(context.Request.Params["StateID"]),
                            responseType,
                            PublicMethods.parse_guid(context.Request.Params["RefStateID"]),
                            PublicMethods.parse_guid(context.Request.Params["DirectorNodeID"]),
                            admin,
                            PublicMethods.parse_guid(context.Request.Params["DirectorUserID"]),
                            ref responseText);
                        _return_response(ref responseText);
                    }
                    return;
                case "SetStatePoll":
                    set_state_poll(workFlowId, stateId,
                        PublicMethods.parse_guid(context.Request.Params["PollID"]), ref responseText);
                    _return_response(ref responseText);
                    return;
                case "SetStateDataNeedsType":
                    string strDataNeedsType = string.IsNullOrEmpty(context.Request.Params["DataNeedsType"]) ? string.Empty :
                        context.Request.Params["DataNeedsType"];
                    StateDataNeedsTypes dataNeedsType = new StateDataNeedsTypes();
                    try { dataNeedsType = (StateDataNeedsTypes)Enum.Parse(typeof(StateDataNeedsTypes), strDataNeedsType); }
                    catch { dataNeedsType = new StateDataNeedsTypes(); }

                    set_state_data_needs_type(workFlowId, stateId, dataNeedsType, refStateId, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "SetStateDataNeedsDescription":
                    set_state_data_needs_description(workFlowId, stateId, description, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "SetStateDescriptionNeeded":
                    bool descriptionNeeded = string.IsNullOrEmpty(context.Request.Params["DescriptionNeeded"]) ? true :
                        (context.Request.Params["DescriptionNeeded"].ToLower() == "false" ? false : true);
                    set_state_description_needed(workFlowId, stateId, descriptionNeeded, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "SetStateHideOwnerName":
                    bool hideOwnerName = string.IsNullOrEmpty(context.Request.Params["HideOwnerName"]) ? false :
                        context.Request.Params["HideOwnerName"].ToLower() == "true";
                    set_state_hide_owner_name(workFlowId, stateId, hideOwnerName, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "SetStateEditPermission":
                    bool editPermission = string.IsNullOrEmpty(context.Request.Params["EditPermission"]) ? false :
                        context.Request.Params["EditPermission"].ToLower() == "true";
                    set_state_edit_permission(workFlowId, stateId, editPermission, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "SetFreeDataNeedRequests":
                    bool freeDataNeedRequests = string.IsNullOrEmpty(context.Request.Params["FreeDataNeedRequests"]) ? false :
                        context.Request.Params["FreeDataNeedRequests"].ToLower() == "true";
                    set_free_data_need_requests(workFlowId, stateId, freeDataNeedRequests, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "SetStateDataNeed":
                    {
                        Guid previousNodeTypeId = string.IsNullOrEmpty(context.Request.Params["PreviousNodeTypeID"]) ? Guid.Empty :
                            Guid.Parse(context.Request.Params["PreviousNodeTypeID"]);
                        string strMultiselect = string.IsNullOrEmpty(context.Request.Params["MultiSelect"]) ?
                            string.Empty : context.Request.Params["MultiSelect"];
                        bool multiselect = strMultiselect.ToLower() == "true";

                        bool admin = PublicMethods.parse_bool(context.Request.Params["isAdmin"],
                            defaultValue: PublicMethods.parse_bool(context.Request.Params["Admin"], defaultValue: false)).Value;

                        set_state_data_need(workFlowId, stateId, nodeTypeId, previousNodeTypeId, formId, description,
                            multiselect, admin, necessary, ref responseText);
                        _return_response(ref responseText);
                    }
                    return;
                case "RemoveStateDataNeed":
                    remove_state_data_need(workFlowId, stateId, nodeTypeId, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "SetRejectionSettings":
                    int mar = string.IsNullOrEmpty(context.Request.Params["MaxAllowedRejections"]) ? 0 :
                        int.Parse(context.Request.Params["MaxAllowedRejections"]);
                    string rejectionTitle = string.IsNullOrEmpty(context.Request.Params["RejectionTitle"]) ? string.Empty :
                        Base64.decode(context.Request.Params["RejectionTitle"]);
                    Guid rejectionRefStateId = string.IsNullOrEmpty(context.Request.Params["RejectionRefStateID"]) ? Guid.Empty :
                        Guid.Parse(context.Request.Params["RejectionRefStateID"]);
                    set_rejection_settings(workFlowId, stateId, mar, rejectionTitle, rejectionRefStateId, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "SetMaxAllowedRejections":
                    int _mar = string.IsNullOrEmpty(context.Request.Params["MaxAllowedRejections"]) ? 0 :
                        int.Parse(context.Request.Params["MaxAllowedRejections"]);
                    set_max_allowed_rejections(workFlowId, stateId, _mar, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "CreateStateDataNeedInstance":
                    {
                        bool admin = PublicMethods.parse_bool(context.Request.Params["isAdmin"],
                            defaultValue: PublicMethods.parse_bool(context.Request.Params["Admin"], defaultValue: false)).Value;

                        create_state_data_need_instance(historyId, nodeId, admin, formId, ref responseText);
                        _return_response(ref responseText);
                    }
                    return;
                case "GetStateDataNeedInstance":
                    get_state_data_need_instance(PublicMethods.parse_guid(context.Request.Params["InstanceID"]), 
                        ref responseText);
                    _return_response(ref responseText);
                    return;
                case "SetStateDataNeedInstanceAsFilled":
                    set_state_data_need_instance_as_filled(PublicMethods.parse_guid(context.Request.Params["InstanceID"]), 
                        ref responseText);
                    _return_response(ref responseText);
                    return;
                case "SetStateDataNeedInstanceAsNotFilled":
                    set_state_data_need_instance_as_not_filled(PublicMethods.parse_guid(context.Request.Params["InstanceID"]), 
                        ref responseText);
                    _return_response(ref responseText);
                    return;
                case "RemoveStateDataNeedInstance":
                    Guid __instanceId = string.IsNullOrEmpty(context.Request.Params["InstanceID"]) ? Guid.Empty :
                        Guid.Parse(context.Request.Params["InstanceID"]);
                    remove_state_data_need_instance(__instanceId, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "AddStateConnection":
                    add_state_connection(workFlowId, inStateId, outStateId, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "SortStateConnections":
                    sort_state_connections(ListMaker.get_guid_items(context.Request.Params["IDs"], '|'), ref responseText);
                    _return_response(ref responseText);
                    return;
                case "MoveStateConnection":
                    bool moveDown = string.IsNullOrEmpty(context.Request.Params["MoveDown"]) ? false :
                        context.Request.Params["MoveDown"].ToLower() == "true";
                    move_state_connection(workFlowId, inStateId, outStateId, moveDown, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "RemoveStateConnection":
                    remove_state_connection(workFlowId, inStateId, outStateId, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "SetStateConnectionLabel":
                    set_state_connection_label(workFlowId, inStateId, outStateId, label, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "SetStateConnectionAttachmentStatus":
                    set_state_connection_attachment_status(workFlowId,
                        inStateId, outStateId, attachmentRequired, attachmentTitle, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "SetStateConnectionDirector":
                    set_state_connection_director(workFlowId, inStateId, outStateId, nodeRequired, nodeTypeId,
                        description, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "SetStateConnectionForm":
                    set_state_connection_form(workFlowId,
                        inStateId, outStateId, formId, description, necessary, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "RemoveStateConnectionForm":
                    remove_state_connection_form(workFlowId, inStateId, outStateId, formId, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "SetAutoMessage":
                    {
                        Guid _automessageId = string.IsNullOrEmpty(context.Request.Params["AutoMessageID"]) ? Guid.Empty :
                            Guid.Parse(context.Request.Params["AutoMessageID"]);

                        AudienceTypes audienceType = PublicMethods.parse_enum<AudienceTypes>(
                            context.Request.Params["AudienceType"], defaultValue: AudienceTypes.SendToOwner);

                        bool admin = PublicMethods.parse_bool(context.Request.Params["isAdmin"],
                            defaultValue: PublicMethods.parse_bool(context.Request.Params["Admin"], defaultValue: false)).Value;

                        if (_automessageId == Guid.Empty)
                        {
                            add_auto_message(PublicMethods.parse_guid(context.Request.Params["OwnerID"]),
                                PublicMethods.parse_string(context.Request.Params["BodyText"]),
                                audienceType, refStateId, nodeId, admin, ref responseText);
                        }
                        else
                        {
                            modify_auto_message(_automessageId,
                                PublicMethods.parse_string(context.Request.Params["BodyText"]),
                                audienceType, refStateId, nodeId, admin, ref responseText);
                        }

                        _return_response(ref responseText);
                    }
                    return;
                case "RemoveAutoMessage":
                    Guid automessageId = string.IsNullOrEmpty(context.Request.Params["AutoMessageID"]) ? Guid.Empty :
                        Guid.Parse(context.Request.Params["AutoMessageID"]);
                    remove_auto_message(automessageId, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "GetOwnerHistory":
                    bool lastOnly = !string.IsNullOrEmpty(context.Request.Params["LastOnly"]) && 
                        context.Request.Params["LastOnly"].ToLower() == "true";
                    bool done = !string.IsNullOrEmpty(context.Request.Params["Done"]) &&
                        context.Request.Params["Done"].ToLower() == "true";
                    get_owner_history(ownerId, lastOnly, done, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "GetWorkFlowOptions":
                    get_workflow_options(ownerId, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "CreateHistoryFormInstance":
                    create_history_form_instance(historyId, outStateId, formId, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "SendToNextState":
                    bool reject = string.IsNullOrEmpty(context.Request.Params["Reject"]) ? false :
                        context.Request.Params["Reject"].ToLower() == "true";
                    send_to_next_state(historyId, stateId, PublicMethods.parse_guid(context.Request.Params["NodeID"]),
                        description, ref attachedFiles, reject, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "TerminateWorkFlow":
                    terminate_workflow(historyId, description, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "RestartWorkFlow":
                    restart_workflow(ownerId, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "GetWorkFlowOwners":
                    get_workflow_owners(ref responseText);
                    _return_response(ref responseText);
                    return;
                case "GetServiceAbstract":
                    string emptyTagLabel = string.IsNullOrEmpty(context.Request.Params["EmptyTagLabel"]) ?
                        string.Empty : context.Request.Params["EmptyTagLabel"].Replace(' ', '+');
                    Base64.decode(emptyTagLabel, ref emptyTagLabel);
                    if (string.IsNullOrEmpty(emptyTagLabel)) emptyTagLabel = "بدون تگ";
                    get_service_abstract(nodeTypeId, workFlowId, userId, emptyTagLabel, false, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "AddOwnerWorkFlow":
                    add_owner_workflow(nodeTypeId, workFlowId, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "RemoveOwnerWorkFlow":
                    remove_owner_workflow(nodeTypeId, workFlowId, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "GetOwnerWorkFlows":
                    get_owner_workflows(nodeTypeId, ref responseText);
                    _return_response(ref responseText);
                    return;
                case "GetOwnerWorkFlowPrimaryKey":
                    get_owner_workflow_primary_key(nodeTypeId, workFlowId, ref responseText);
                    _return_response(ref responseText);
                    return;
            }

            paramsContainer.return_response(PublicConsts.BadRequestResponse);
        }

        protected void _return_response(ref string responseText)
        {
            paramsContainer.return_response(ref responseText);
        }

        protected void _save_error_log(Modules.Log.Action action, List<Guid> subjectIds,
            Guid? secondSubjectId = null, Guid? thirdSubjectId = null, string info = null)
        {
            try
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = action,
                    SubjectIDs = subjectIds,
                    SecondSubjectID = secondSubjectId,
                    ThirdSubjectID = thirdSubjectId,
                    Info = info,
                    ModuleIdentifier = ModuleIdentifier.WF
                });
            }
            catch { }
        }

        protected void _save_error_log(Modules.Log.Action action, Guid? subjectId,
            Guid? secondSubjectId = null, Guid? thirdSubjectId = null, string info = null)
        {
            if (!subjectId.HasValue) return;
            _save_error_log(action, new List<Guid>() { subjectId.Value }, secondSubjectId, thirdSubjectId, info);
        }

        protected bool _has_workflow_permission(Guid nodeId, bool? isSystemAdmin = null,
            bool? isServiceAdmin = null, Modules.CoreNetwork.Node node = null)
        {
            if (node == null) node = CNController.get_node(paramsContainer.Tenant.Id, nodeId);

            if (!isSystemAdmin.HasValue) isSystemAdmin =
                    PublicMethods.is_system_admin(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID.Value);

            if (!isServiceAdmin.HasValue) isServiceAdmin =
                    CNController.is_service_admin(paramsContainer.Tenant.Id, nodeId, paramsContainer.CurrentUserID.Value);

            bool hasKPermission = false, hasWFPermission = false,
                hasWFEditPermission = false, hideContributors = false;

            (new CNAPI() { paramsContainer = this.paramsContainer })
                    .check_node_workflow_permissions(node, false, isSystemAdmin.Value, isServiceAdmin.Value,
                    false, false, ref hasKPermission, ref hasWFPermission, ref hasWFEditPermission, ref hideContributors);

            return hasWFPermission;
        }

        protected Dictionary<string, string> _get_replacement_dictionary(Guid nodeId,
            string comment, Guid currentUserId, ref Modules.CoreNetwork.Node node)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();

            node = CNController.get_node(paramsContainer.Tenant.Id, nodeId, true);
            if (node == null) return dic;

            User _user = UsersController.get_user(paramsContainer.Tenant.Id, currentUserId);

            string nodeName = "'" + node.Name + "'";
            string additionalId = "'" + node.AdditionalID + "'";
            string creator = "'" + node.Creator.FirstName + " " + node.Creator.LastName + " - " + node.Creator.UserName + "'";
            string creationDate = "'" + (node.CreationDate.HasValue ? GenericDate.get_local_date(node.CreationDate.Value, true) : "___") + "'";

            if (string.IsNullOrEmpty(comment)) comment = string.Empty;

            dic.Add("Comment", comment);
            dic.Add("NodeName", nodeName);
            dic.Add("AdditionalID", additionalId);
            dic.Add("Creator", creator);
            dic.Add("CreationDate", creationDate);
            dic.Add("CurrentUser", _user == null ? string.Empty : _user.FirstName + " " + _user.LastName + " - " + _user.UserName);
            dic.Add("Now", GenericDate.get_local_date(DateTime.Now, true));

            return dic;
        }

        protected void create_state(string title, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!string.IsNullOrEmpty(title) && title.Length > 250)
            {
                responseText = "{\"ErrorText\":\"" + Messages.MaxAllowedInputLengthExceeded + "\"}";
                return;
            }
            else if (!PublicMethods.is_secure_title(title))
            {
                responseText = "{\"ErrorText\":\"" + Messages.TheTextIsFormattedBadly + "\"}";
                return;
            }

            if (!AuthorizationManager.has_right(AccessRoleName.ManageWorkflow, paramsContainer.CurrentUserID))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                _save_error_log(Modules.Log.Action.CreateState_PermissionFailed, Guid.Empty);
                return;
            }

            State state = new State()
            {
                StateID = Guid.NewGuid(),
                Title = title,
                CreatorUserID = paramsContainer.CurrentUserID,
                CreationDate = DateTime.Now
            };

            bool succeed = WFController.create_state(paramsContainer.Tenant.Id, state);

            responseText = !succeed ? "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}" :
                "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully +
                "\",\"State\":{\"StateID\":\"" + state.StateID.Value.ToString() + "\",\"Title\":\"" + Base64.encode(state.Title) + "\"}}";

            //Save Log
            if (succeed)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = state.CreationDate,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.CreateState,
                    SubjectID = state.StateID,
                    Info = "{\"Name\":\"" + Base64.encode(state.Title) + "\"}",
                    ModuleIdentifier = ModuleIdentifier.WF
                });
            }
            //end of Save Log
        }

        protected void modify_state(Guid stateId, string title, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!string.IsNullOrEmpty(title) && title.Length > 250)
            {
                responseText = "{\"ErrorText\":\"" + Messages.MaxAllowedInputLengthExceeded + "\"}";
                return;
            }
            else if (!PublicMethods.is_secure_title(title))
            {
                responseText = "{\"ErrorText\":\"" + Messages.TheTextIsFormattedBadly + "\"}";
                return;
            }

            if (!AuthorizationManager.has_right(AccessRoleName.ManageWorkflow, paramsContainer.CurrentUserID))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                _save_error_log(Modules.Log.Action.ModifyState_PermissionFailed, stateId);
                return;
            }

            State state = new State()
            {
                StateID = stateId,
                Title = title,
                LastModifierUserID = paramsContainer.CurrentUserID,
                LastModificationDate = DateTime.Now
            };

            bool succeed = WFController.modify_state(paramsContainer.Tenant.Id, state);

            responseText = succeed ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";

            //Save Log
            if (succeed)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = state.LastModificationDate,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.ModifyState,
                    SubjectID = state.StateID,
                    Info = "{\"Name\":\"" + Base64.encode(state.Title) + "\"}",
                    ModuleIdentifier = ModuleIdentifier.WF
                });
            }
            //end of Save Log
        }

        protected void remove_state(Guid stateId, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!AuthorizationManager.has_right(AccessRoleName.ManageWorkflow, paramsContainer.CurrentUserID))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                _save_error_log(Modules.Log.Action.RemoveState_PermissionFailed, stateId);
                return;
            }

            bool succeed = WFController.remove_state(paramsContainer.Tenant.Id,
                stateId, paramsContainer.CurrentUserID.Value);

            responseText = succeed ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";

            //Save Log
            if (succeed)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = DateTime.Now,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.RemoveState,
                    SubjectID = stateId,
                    ModuleIdentifier = ModuleIdentifier.WF
                });
            }
            //end of Save Log
        }

        protected void get_states(Guid workflowId, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            List<State> states = WFController.get_states(paramsContainer.Tenant.Id, workflowId);

            responseText = "{\"States\":[";

            bool isFirst = true;
            foreach (State st in states)
            {
                string title = st.Title;
                Base64.encode(title, ref title);

                if (!isFirst) responseText += ",";
                isFirst = false;

                responseText += "{\"StateID\":\"" + st.StateID.ToString() + "\",\"Title\":\"" + title + "\"}";
            }

            responseText += "]}";
        }

        protected void create_workflow(string name, string description, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if ((!string.IsNullOrEmpty(name) && name.Length > 250) ||
                (!string.IsNullOrEmpty(description) && description.Length > 1900))
            {
                responseText = "{\"ErrorText\":\"" + Messages.MaxAllowedInputLengthExceeded + "\"}";
                return;
            }
            else if (!PublicMethods.is_secure_title(name))
            {
                responseText = "{\"ErrorText\":\"" + Messages.TheTextIsFormattedBadly + "\"}";
                return;
            }

            if (!AuthorizationManager.has_right(AccessRoleName.ManageWorkflow, paramsContainer.CurrentUserID))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                _save_error_log(Modules.Log.Action.CreateWorkFlow_PermissionFailed, Guid.Empty);
                return;
            }

            WorkFlow workFlow = new WorkFlow()
            {
                WorkFlowID = Guid.NewGuid(),
                Name = name,
                Description = description,
                CreatorUserID = paramsContainer.CurrentUserID,
                CreationDate = DateTime.Now
            };

            bool succeed = WFController.create_workflow(paramsContainer.Tenant.Id, workFlow);

            responseText = !succeed ? "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}" :
                "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"" + ",\"WorkFlow\":" + workFlow.toJson() + "}";

            //Save Log
            if (succeed)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = workFlow.CreationDate,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.CreateWorkFlow,
                    SubjectID = workFlow.WorkFlowID,
                    Info = "{\"Name\":\"" + Base64.encode(workFlow.Name) + "\",\"Description\":\"" + Base64.encode(workFlow.Description) + "\"}",
                    ModuleIdentifier = ModuleIdentifier.WF
                });
            }
            //end of Save Log
        }

        protected void modify_workflow(Guid workflowId, string name, string description, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if ((!string.IsNullOrEmpty(name) && name.Length > 250) ||
                (!string.IsNullOrEmpty(description) && description.Length > 1900))
            {
                responseText = "{\"ErrorText\":\"" + Messages.MaxAllowedInputLengthExceeded + "\"}";
                return;
            }
            else if (!PublicMethods.is_secure_title(name))
            {
                responseText = "{\"ErrorText\":\"" + Messages.TheTextIsFormattedBadly + "\"}";
                return;
            }

            if (!AuthorizationManager.has_right(AccessRoleName.ManageWorkflow, paramsContainer.CurrentUserID))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                _save_error_log(Modules.Log.Action.ModifyWorkFlow_PermissionFailed, workflowId);
                return;
            }

            WorkFlow workFlow = new WorkFlow()
            {
                WorkFlowID = workflowId,
                Name = name,
                Description = description,
                LastModifierUserID = paramsContainer.CurrentUserID,
                LastModificationDate = DateTime.Now
            };

            bool succeed = WFController.modify_workflow(paramsContainer.Tenant.Id, workFlow);

            responseText = succeed ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";

            //Save Log
            if (succeed)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = workFlow.LastModificationDate,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.ModifyWorkFlow,
                    SubjectID = workFlow.WorkFlowID,
                    Info = "{\"Name\":\"" + Base64.encode(workFlow.Name) + "\",\"Description\":\"" + Base64.encode(workFlow.Description) + "\"}",
                    ModuleIdentifier = ModuleIdentifier.WF
                });
            }
            //end of Save Log
        }

        protected void remove_workflow(Guid workflowId, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!AuthorizationManager.has_right(AccessRoleName.ManageWorkflow, paramsContainer.CurrentUserID))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                _save_error_log(Modules.Log.Action.RemoveWorkFlow_PermissionFailed, workflowId);
                return;
            }

            int curItemsCount = WFController.get_workflow_items_count(paramsContainer.Tenant.Id, workflowId);

            if (curItemsCount > 0)
            {
                responseText = "{\"ErrorText\":\"" + Messages.CurrentlyThisWorkFlowContainsNItems + "\"" +
                    ",\"ItemsCount\":" + curItemsCount.ToString() + "}";
                return;
            }

            bool succeed = WFController.remove_workflow(paramsContainer.Tenant.Id,
                workflowId, paramsContainer.CurrentUserID.Value);

            responseText = succeed ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";

            //Save Log
            if (succeed)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = DateTime.Now,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.RemoveWorkFlow,
                    SubjectID = workflowId,
                    ModuleIdentifier = ModuleIdentifier.WF
                });
            }
            //end of Save Log
        }

        protected void get_workflows(ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            List<WorkFlow> workflows = WFController.get_workflows(paramsContainer.Tenant.Id);

            responseText = "{\"WorkFlows\":[" + string.Join(",", workflows.Select(u => u.toJson())) + "]}";
        }

        protected string _get_auto_message_json(AutoMessage connectionAudience)
        {
            return "{\"AutoMessageID\":\"" + connectionAudience.AutoMessageID.Value.ToString() + "\"" +
                ",\"OwnerID\":\"" + connectionAudience.OwnerID.Value.ToString() + "\"" +
                ",\"BodyText\":\"" + Base64.encode(connectionAudience.BodyText) + "\"" +
                ",\"AudienceType\":\"" + connectionAudience.AudienceType + "\"" +
                ",\"RefStateID\":\"" + (connectionAudience.RefState.StateID.HasValue ?
                    connectionAudience.RefState.StateID.Value.ToString() : string.Empty) + "\"" +
                ",\"RefStateTitle\":\"" + Base64.encode(connectionAudience.RefState.Title) + "\"" +
                ",\"NodeID\":\"" + (connectionAudience.Node.NodeID.HasValue ?
                    connectionAudience.Node.NodeID.Value.ToString() : string.Empty) + "\"" +
                ",\"NodeName\":\"" + Base64.encode(connectionAudience.Node.Name) + "\"" +
                ",\"NodeTypeID\":\"" + (connectionAudience.Node.NodeTypeID.HasValue ?
                    connectionAudience.Node.NodeTypeID.Value.ToString() : string.Empty) + "\"" +
                ",\"NodeType\":\"" + Base64.encode(connectionAudience.Node.NodeType) + "\"" +
                ",\"Admin\":" + (connectionAudience.Admin.HasValue ?
                    connectionAudience.Admin.Value : false).ToString().ToLower() +
                "}";
        }

        protected string _get_connection_form_json(StateConnectionForm connectionForm, ref Dictionary<Guid, string> names)
        {
            string title = connectionForm.Form.Title;
            string description = connectionForm.Description;

            Base64.encode(title, ref title);
            Base64.encode(description, ref description);

            return "{\"FormID\":\"" + (connectionForm.Form.FormID.HasValue ? connectionForm.Form.FormID.Value.ToString() : string.Empty) +
                "\",\"Title\":\"" + title + "\",\"Description\":\"" + description +
                "\",\"Necessary\":" + (connectionForm.Necessary.HasValue ? connectionForm.Necessary.Value : false).ToString().ToLower() + "}";
        }

        protected string _get_connection_json(StateConnection connection, ref Dictionary<Guid, string> names)
        {
            string outStateTitle = Base64.encode(names[connection.OutState.StateID.Value]);
            string nodeTypeName = string.Empty;
            try
            {
                nodeTypeName = connection.DirectorNodeType.NodeTypeID.HasValue ?
                    Base64.encode(names[connection.DirectorNodeType.NodeTypeID.Value]) : string.Empty;
            }
            catch { nodeTypeName = string.Empty; }

            string str = "{\"ID\":\"" + connection.ID.ToString() + "\"" +
                ",\"OutStateID\":\"" + connection.OutState.StateID.Value.ToString() + "\"" +
                ",\"OutStateTitle\":\"" + outStateTitle + "\"" +
                ",\"ConnectionLabel\":\"" + Base64.encode(connection.Label) + "\"" +
                ",\"AttachmentRequired\":" + (connection.AttachmentRequired.HasValue ?
                    connection.AttachmentRequired.Value : false).ToString().ToLower() +
                ",\"AttachmentTitle\":\"" + Base64.encode(connection.AttachmentTitle) + "\"" +
                ",\"NodeRequired\":" + (connection.NodeRequired.HasValue ? connection.NodeRequired.Value : false).ToString().ToLower() +
                ",\"NodeTypeID\":\"" + (connection.DirectorNodeType.NodeTypeID.HasValue ?
                    connection.DirectorNodeType.NodeTypeID.Value.ToString() : string.Empty) + "\"" +
                ",\"NodeTypeName\":\"" + nodeTypeName + "\"" +
                ",\"NodeTypeDescription\":\"" + Base64.encode(connection.NodeTypeDescription) + "\"" +
                ",\"AttachedFiles\":[" + string.Join(",",
                    connection.AttachedFiles.Select(u => u.toJson())) + "]";

            str += ",\"Forms\":[";

            bool isFirst = true;
            foreach (StateConnectionForm scf in connection.Forms)
            {
                str += (isFirst ? string.Empty : ",") + _get_connection_form_json(scf, ref names);
                isFirst = false;
            }

            str += "],\"Audience\":[";

            isFirst = true;
            foreach (AutoMessage sca in connection.AutoMessages)
            {
                str += (isFirst ? string.Empty : ",") + _get_auto_message_json(sca);
                isFirst = false;
            }

            str += "]}";

            return str;
        }

        protected string _get_state_data_need_json(StateDataNeed need, ref Dictionary<Guid, string> names)
        {
            string nodeTypeName = need.DirectorNodeType.NodeTypeID.HasValue ?
                    Base64.encode(names[need.DirectorNodeType.NodeTypeID.Value]) : string.Empty;

            return "{\"ID\":\"" + need.ID.ToString() + "\"" +
                ",\"NodeTypeID\":\"" + need.DirectorNodeType.NodeTypeID.Value.ToString() + "\"" +
                ",\"NodeType\":\"" + nodeTypeName + "\"" +
                (!need.FormID.HasValue ? string.Empty :
                    ",\"FormID\":\"" + need.FormID.Value.ToString() + "\"" +
                    ",\"FormTitle\":\"" + Base64.encode(need.FormTitle) + "\""
                ) +
                ",\"MultiSelect\":" + (need.MultiSelect.HasValue ? need.MultiSelect.Value : false).ToString().ToLower() +
                ",\"Admin\":" + (need.Admin.HasValue ? need.Admin.Value : false).ToString().ToLower() +
                ",\"Necessary\":" + (need.Necessary.HasValue ? need.Necessary.Value : false).ToString().ToLower() +
                ",\"Description\":\"" + Base64.encode(need.Description) + "\"}";
        }

        protected string _get_state_json(State state, ref Dictionary<Guid, string> names)
        {
            string stateTitle = Base64.encode(string.IsNullOrEmpty(state.Title) ? names[state.StateID.Value] : state.Title);
            string refDataNeedsStateTitle = state.RefDataNeedsStateID.HasValue ?
                Base64.encode(names[state.RefDataNeedsStateID.Value]) : string.Empty;
            string refDirectorStateTitle = state.RefStateID.HasValue ? Base64.encode(names[state.RefStateID.Value]) : string.Empty;

            string strDirector = "{\"ResponseType\":\"" + (state.ResponseType.HasValue ? state.ResponseType.ToString() : string.Empty) + "\"" +
                ",\"RefStateID\":\"" + (state.RefStateID.HasValue ? state.RefStateID.Value.ToString() : string.Empty) + "\"" +
                ",\"RefStateTitle\":\"" + refDirectorStateTitle + "\"" +
                ",\"NodeName\":\"" + Base64.encode(state.DirectorNode.Name) + "\"" +
                ",\"NodeID\":\"" + (state.DirectorNode.NodeID.HasValue ? state.DirectorNode.NodeID.Value.ToString() : string.Empty) + "\"" +
                ",\"NodeTypeID\":\"" + (state.DirectorNode.NodeTypeID.HasValue ?
                    state.DirectorNode.NodeTypeID.Value.ToString() : string.Empty) + "\"" +
                ",\"NodeType\":\"" + Base64.encode(state.DirectorNode.NodeType) + "\"" +
                ",\"Admin\":" + (state.DirectorIsAdmin.HasValue ? state.DirectorIsAdmin.Value : false).ToString().ToLower() +
                ",\"FullName\":\"" + Base64.encode(state.DirectorUser.FullName) + "\"" +
                ",\"UserID\":\"" + (state.DirectorUser.UserID.HasValue ? state.DirectorUser.UserID.Value.ToString() : string.Empty) + "\"" +
                "}";

            string str = "{\"ID\":\"" + state.ID.ToString() + "\"" +
                ",\"StateID\":\"" + state.StateID.Value.ToString() + "\"" +
                ",\"Title\":\"" + stateTitle + "\"" +
                ",\"Description\":\"" + Base64.encode(state.Description) + "\"" +
                ",\"Tag\":\"" + Base64.encode(state.Tag) + "\"" +
                ",\"DataNeedsType\":\"" + (state.DataNeedsType.HasValue ? state.DataNeedsType.ToString() : string.Empty) + "\"" +
                ",\"RefDataNeedsStateID\":\"" + (state.RefDataNeedsStateID.HasValue ?
                    state.RefDataNeedsStateID.ToString() : string.Empty) + "\"" +
                ",\"RefDataNeedsStateTitle\":\"" + refDataNeedsStateTitle + "\"" +
                ",\"DataNeedsDescription\":\"" + Base64.encode(state.DataNeedsDescription) + "\"" +
                ",\"DescriptionNeeded\":" + (state.DescriptionNeeded.HasValue ?
                    state.DescriptionNeeded.Value.ToString().ToLower() : "true") +
                ",\"HideOwnerName\":" + (state.HideOwnerName.HasValue ? state.HideOwnerName.Value.ToString().ToLower() : "false") +
                ",\"EditPermission\":" + (state.EditPermission.HasValue ? state.EditPermission.Value.ToString().ToLower() : "false") +
                ",\"FreeDataNeedRequests\":" + (state.FreeDataNeedRequests.HasValue ?
                    state.FreeDataNeedRequests.Value.ToString().ToLower() : "false") +
                ",\"Director\":" + strDirector +
                ",\"MaxAllowedRejections\":" + (state.MaxAllowedRejections.HasValue ? state.MaxAllowedRejections.ToString() : "null") +
                ",\"RejectionTitle\":\"" + Base64.encode(state.RejectionTitle) + "\"" +
                ",\"RejectionRefStateID\":\"" + (state.RejectionRefStateID.HasValue ?
                    state.RejectionRefStateID.ToString() : string.Empty) + "\"" +
                ",\"RejectionRefStateTitle\":\"" + Base64.encode(state.RejectionRefStateTitle) + "\"" +
                (!state.PollID.HasValue ? string.Empty : ",\"Poll\":{\"PollID\":\"" + state.PollID.ToString() + "\"" +
                    ",\"Name\":\"" + Base64.encode(state.PollName) + "\"" + "}") +
                ",\"DataNeeds\":[";

            bool isFirst = true;
            foreach (StateDataNeed dn in state.DataNeeds)
            {
                str += (isFirst ? string.Empty : ",") + _get_state_data_need_json(dn, ref names);
                isFirst = false;
            }

            str += "],\"Connections\":[";

            isFirst = true;
            foreach (StateConnection conn in state.Connections)
            {
                str += (isFirst ? string.Empty : ",") + _get_connection_json(conn, ref names);
                isFirst = false;
            }

            str += "]}";

            return str;
        }

        protected string _get_workflow_json(WorkFlow workflow, ref Dictionary<Guid, string> names)
        {
            string str = "{\"WorkFlowID\":\"" + workflow.WorkFlowID.Value.ToString() + "\"" +
                ",\"Name\":\"" + Base64.encode(workflow.Name) + "\"" +
                ",\"Description\":\"" + Base64.encode(workflow.Description) + "\"" +
                ",\"States\":[";

            bool isFirst = true;
            foreach (State st in workflow.States)
            {
                str += (isFirst ? string.Empty : ",") + _get_state_json(st, ref names);
                isFirst = false;
            }

            str += "]}";

            return str;
        }

        protected void get_workflow(Guid workFlowId, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!AuthorizationManager.has_right(AccessRoleName.ManageWorkflow, paramsContainer.CurrentUserID))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                _save_error_log(Modules.Log.Action.GetWorkFlow_PermissionFailed, workFlowId);
                return;
            }

            WorkFlow workflow = WFController.get_workflow(paramsContainer.Tenant.Id, workFlowId);

            List<State> states = WFController.get_workflow_states(paramsContainer.Tenant.Id, workFlowId);
            List<StateDataNeed> stateDataNeeds = WFController.get_state_data_needs(paramsContainer.Tenant.Id, workFlowId);
            List<StateConnection> connections = WFController.get_workflow_connections(paramsContainer.Tenant.Id, workFlowId);
            List<StateConnectionForm> connectionForms =
                WFController.get_workflow_connection_forms(paramsContainer.Tenant.Id, workFlowId);
            List<AutoMessage> automessages = WFController.get_workflow_auto_messages(paramsContainer.Tenant.Id, workFlowId);

            List<Guid> attIds = new List<Guid>();
            foreach (StateConnection sc in connections)
                if (sc.ID.HasValue) attIds.Add(sc.ID.Value);

            List<DocFileInfo> attachedFiles = DocumentsController.get_owner_files(paramsContainer.Tenant.Id, attIds);

            foreach (StateConnection conn in connections)
            {
                conn.Forms = connectionForms.Where(u => u.WorkFlowID == conn.WorkFlowID && u.InStateID == conn.InState.StateID &&
                    u.OutStateID == conn.OutState.StateID).ToList();

                conn.AutoMessages = automessages.Where(v => v.OwnerID == conn.ID).ToList();
                conn.AttachedFiles = attachedFiles.Where(x => x.OwnerID == conn.ID).ToList();
            }

            foreach (State st in states)
            {
                st.DataNeeds = stateDataNeeds.Where(v => v.WorkFlowID == st.WorkFlowID && v.StateID == st.StateID).ToList();
                st.Connections = connections.Where(v => v.WorkFlowID == st.WorkFlowID &&
                    v.InState.StateID == st.StateID)/*.OrderBy(u => u.SequenceNumber)*/.ToList();
            }

            workflow.States = states;

            List<Guid> stateIds = new List<Guid>();
            List<Guid> nodeTypeIds = new List<Guid>();

            Dictionary<Guid, string> names = new Dictionary<Guid, string>();

            stateIds.AddRange(states.Where(u => u.StateID.HasValue).Select(v => v.StateID.Value));
            stateIds.AddRange(states.Where(u => u.RefStateID.HasValue).Select(v => v.RefStateID.Value));
            stateIds.AddRange(states.Where(u => u.RefDataNeedsStateID.HasValue).Select(v => v.RefDataNeedsStateID.Value));
            stateIds.AddRange(connections.Where(u => u.OutState.StateID.HasValue).Select(v => v.OutState.StateID.Value));

            nodeTypeIds.AddRange(stateDataNeeds.Where(u => u.DirectorNodeType.NodeTypeID.HasValue).Select(
                v => v.DirectorNodeType.NodeTypeID.Value));
            nodeTypeIds.AddRange(connections.Where(u => u.DirectorNodeType.NodeTypeID.HasValue).Select(
                v => v.DirectorNodeType.NodeTypeID.Value));

            List<State> _sts = WFController.get_states(paramsContainer.Tenant.Id, stateIds);
            List<NodeType> _ndtps = CNController.get_node_types(paramsContainer.Tenant.Id, nodeTypeIds);

            foreach (State st in _sts) names[st.StateID.Value] = st.Title;
            foreach (NodeType ndtp in _ndtps) names[ndtp.NodeTypeID.Value] = ndtp.Name;

            responseText = _get_workflow_json(workflow, ref names);
        }

        protected void add_workflow_state(Guid workflowId, Guid stateId, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!AuthorizationManager.has_right(AccessRoleName.ManageWorkflow, paramsContainer.CurrentUserID))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                _save_error_log(Modules.Log.Action.AddWorkFlowState_PermissionFailed, workflowId, stateId);
                return;
            }

            Guid id = Guid.NewGuid();

            bool succeed = WFController.add_workflow_state(paramsContainer.Tenant.Id,
                id, workflowId, stateId, paramsContainer.CurrentUserID.Value);

            responseText = !succeed ? "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}" :
                "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\",\"ID\":\"" + id.ToString() + "\"}";

            //Save Log
            if (succeed)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = DateTime.Now,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.AddWorkFlowState,
                    SubjectID = workflowId,
                    SecondSubjectID = stateId,
                    ModuleIdentifier = ModuleIdentifier.WF
                });
            }
            //end of Save Log
        }

        protected void remove_workflow_state(Guid workflowId, Guid stateId, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!AuthorizationManager.has_right(AccessRoleName.ManageWorkflow, paramsContainer.CurrentUserID))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                _save_error_log(Modules.Log.Action.RemoveWorkFlowState_PermissionFailed, workflowId, stateId);
                return;
            }

            int curItemsCount =
                WFController.get_workflow_state_items_count(paramsContainer.Tenant.Id, workflowId, stateId);

            if (curItemsCount > 0)
            {
                responseText = "{\"ErrorText\":\"" + Messages.CurrentlyThisStateContainsNItems + "\"" +
                    ",\"ItemsCount\":" + curItemsCount.ToString() + "}";
                return;
            }

            bool succeed = WFController.remove_workflow_state(paramsContainer.Tenant.Id,
                workflowId, stateId, paramsContainer.CurrentUserID.Value);

            responseText = succeed ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
               "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";

            //Save Log
            if (succeed)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = DateTime.Now,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.RemoveWorkFlowState,
                    SubjectID = workflowId,
                    SecondSubjectID = stateId,
                    ModuleIdentifier = ModuleIdentifier.WF
                });
            }
            //end of Save Log
        }

        protected void set_workflow_state_description(Guid workflowId, Guid stateId, string description, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!string.IsNullOrEmpty(description) && description.Length > 1900)
            {
                responseText = "{\"ErrorText\":\"" + Messages.MaxAllowedInputLengthExceeded + "\"}";
                return;
            }

            if (!AuthorizationManager.has_right(AccessRoleName.ManageWorkflow, paramsContainer.CurrentUserID))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                _save_error_log(Modules.Log.Action.SetWorkFlowStateDataNeedsDescription_PermissionFailed, workflowId, stateId);
                return;
            }

            State state = new State()
            {
                WorkFlowID = workflowId,
                StateID = stateId,
                Description = description,
                LastModifierUserID = paramsContainer.CurrentUserID,
                LastModificationDate = DateTime.Now
            };

            bool succeed = WFController.set_workflow_state_description(paramsContainer.Tenant.Id, state);

            responseText = succeed ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";

            //Save Log
            if (succeed)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = state.LastModificationDate,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.ModifyWorkFlowStateDescription,
                    SubjectID = workflowId,
                    SecondSubjectID = stateId,
                    Info = "{\"Description\":\"" + Base64.encode(description) + "\"}",
                    ModuleIdentifier = ModuleIdentifier.WF
                });
            }
            //end of Save Log
        }

        protected void set_workflow_state_tag(Guid workflowId, Guid stateId, string tag, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!string.IsNullOrEmpty(tag) && tag.Length > 390)
            {
                responseText = "{\"ErrorText\":\"" + Messages.MaxAllowedInputLengthExceeded + "\"}";
                return;
            }
            else if (!PublicMethods.is_secure_title(tag))
            {
                responseText = "{\"ErrorText\":\"" + Messages.TheTextIsFormattedBadly + "\"}";
                return;
            }

            if (!AuthorizationManager.has_right(AccessRoleName.ManageWorkflow, paramsContainer.CurrentUserID))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                _save_error_log(Modules.Log.Action.SetWorkFlowStateTag_PermissionFailed, workflowId, stateId);
                return;
            }

            bool succeed = WFController.set_workflow_state_tag(paramsContainer.Tenant.Id,
                workflowId, stateId, tag, paramsContainer.CurrentUserID.Value);

            responseText = succeed ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";

            //Save Log
            if (succeed)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = DateTime.Now,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.SetWorkFlowStateTag,
                    SubjectID = workflowId,
                    SecondSubjectID = stateId,
                    Info = "{\"Tag\":\"" + Base64.encode(tag) + "\"}",
                    ModuleIdentifier = ModuleIdentifier.WF
                });
            }
            //end of Save Log
        }

        protected void remove_workflow_state_tag(Guid workflowId, Guid stateId, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!AuthorizationManager.has_right(AccessRoleName.ManageWorkflow, paramsContainer.CurrentUserID))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                _save_error_log(Modules.Log.Action.RemoveWorkFlowStateTag_PermissionFailed, workflowId, stateId);
                return;
            }

            bool succeed = WFController.remove_workflow_state_tag(paramsContainer.Tenant.Id, workflowId, stateId);

            responseText = succeed ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";

            //Save Log
            if (succeed)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = DateTime.Now,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.RemoveWorkFlowStateTag,
                    SubjectID = workflowId,
                    SecondSubjectID = stateId,
                    ModuleIdentifier = ModuleIdentifier.WF
                });
            }
            //end of Save Log
        }

        protected void get_workflow_tags(Guid workflowId, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            List<Tag> tags = WFController.get_workflow_tags(paramsContainer.Tenant.Id, workflowId);

            responseText = "{\"Tags\":[";

            bool isFirst = true;
            foreach (Tag tg in tags)
            {
                responseText += (isFirst ? string.Empty : ",") +
                    "{\"TagID\":\"" + tg.TagID.ToString() + "\",\"Tag\":\"" + Base64.encode(tg.Text) + "\"}";
                isFirst = false;
            }

            responseText += "]}";
        }

        protected void set_state_director(Guid? workflowId, Guid? stateId, StateResponseTypes responseType, 
            Guid? refStateId, Guid? directorNodeId, bool admin, Guid? directorUserId, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!AuthorizationManager.has_right(AccessRoleName.ManageWorkflow, paramsContainer.CurrentUserID))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                _save_error_log(Modules.Log.Action.SetWorkFlowStateDirector_PermissionFailed, workflowId, stateId);
                return;
            }

            State state = new State()
            {
                WorkFlowID = workflowId,
                StateID = stateId,
                ResponseType = responseType,
                RefStateID = refStateId,
                DirectorIsAdmin = admin,
                CreatorUserID = paramsContainer.CurrentUserID,
                CreationDate = DateTime.Now,
                DirectorNode = new Node() { NodeID = directorNodeId },
                DirectorUser = new User() { UserID = directorUserId }
            };

            bool succeed = workflowId.HasValue && stateId.HasValue &&
                WFController.set_state_director(paramsContainer.Tenant.Id, state);

            responseText = succeed ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";

            //Save Log
            if (succeed)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = state.CreationDate,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.SetWorkFlowStateDirector,
                    SubjectID = workflowId,
                    SecondSubjectID = stateId,
                    Info = "{\"ResponseType\":\"" + responseType + "\",\"RefStateID\":\"" +
                        (refStateId == Guid.Empty ? string.Empty : refStateId.ToString()) +
                        "\",\"DirectorIsAdmin\":" + admin.ToString().ToLower() + "}",
                    ModuleIdentifier = ModuleIdentifier.WF
                });
            }
            //end of Save Log
        }

        protected void set_state_poll(Guid workflowId, Guid stateId, Guid? pollId, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!AuthorizationManager.has_right(AccessRoleName.ManageWorkflow, paramsContainer.CurrentUserID))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                _save_error_log(Modules.Log.Action.SetWorkFlowStatePoll_PermissionFailed, workflowId, stateId);
                return;
            }

            bool succeed = WFController.set_state_poll(paramsContainer.Tenant.Id,
                workflowId, stateId, pollId, paramsContainer.CurrentUserID.Value);

            responseText = succeed ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";

            //Save Log
            if (succeed)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = DateTime.Now,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.SetWorkFlowStatePoll,
                    SubjectID = workflowId,
                    SecondSubjectID = stateId,
                    ThirdSubjectID = pollId,
                    ModuleIdentifier = ModuleIdentifier.WF
                });
            }
            //end of Save Log
        }

        protected void set_state_data_needs_type(Guid workflowId, Guid stateId, StateDataNeedsTypes dataNeedsType,
            Guid refStateId, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!AuthorizationManager.has_right(AccessRoleName.ManageWorkflow, paramsContainer.CurrentUserID))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                _save_error_log(Modules.Log.Action.SetWorkFlowStateDataNeedsType_PermissionFailed, workflowId, stateId);
                return;
            }

            bool succeed = WFController.set_state_data_needs_type(paramsContainer.Tenant.Id, workflowId,
                stateId, dataNeedsType, refStateId, paramsContainer.CurrentUserID.Value);

            responseText = succeed ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";

            //Save Log
            if (succeed)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = DateTime.Now,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.SetWorkFlowStateDataNeedsType,
                    SubjectID = workflowId,
                    SecondSubjectID = stateId,
                    Info = "{\"DataNeedsType\":\"" + dataNeedsType.ToString() +
                        "\",\"RefStateID\":\"" + (refStateId == Guid.Empty ? string.Empty : refStateId.ToString()) + "\"}",
                    ModuleIdentifier = ModuleIdentifier.WF
                });
            }
            //end of Save Log
        }

        protected void set_state_data_needs_description(Guid workflowId,
            Guid stateId, string description, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!string.IsNullOrEmpty(description) && description.Length > 1900)
            {
                responseText = "{\"ErrorText\":\"" + Messages.MaxAllowedInputLengthExceeded + "\"}";
                return;
            }

            if (!AuthorizationManager.has_right(AccessRoleName.ManageWorkflow, paramsContainer.CurrentUserID))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                _save_error_log(Modules.Log.Action.SetWorkFlowStateDataNeedsDescription_PermissionFailed, workflowId, stateId);
                return;
            }

            bool succeed = WFController.set_state_data_needs_description(paramsContainer.Tenant.Id, workflowId,
                stateId, description, paramsContainer.CurrentUserID.Value);

            responseText = succeed ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";

            //Save Log
            if (succeed)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = DateTime.Now,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.SetWorkFlowStateDataNeedsDescription,
                    SubjectID = workflowId,
                    SecondSubjectID = stateId,
                    Info = "{\"Description\":\"" + description + "\"}",
                    ModuleIdentifier = ModuleIdentifier.WF
                });
            }
            //end of Save Log
        }

        protected void set_state_description_needed(Guid workflowId,
            Guid stateId, bool descriptionNeeded, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!AuthorizationManager.has_right(AccessRoleName.ManageWorkflow, paramsContainer.CurrentUserID))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                _save_error_log(descriptionNeeded ? Modules.Log.Action.SetWorkFlowStateDescriptionAsNeeded_PermissionFailed :
                        Modules.Log.Action.SetWorkFlowStateDescriptionAsNotNeeded_PermissionFailed, workflowId, stateId);
                return;
            }

            bool succeed = WFController.set_state_description_needed(paramsContainer.Tenant.Id, workflowId,
                stateId, descriptionNeeded, paramsContainer.CurrentUserID.Value);

            responseText = succeed ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";

            //Save Log
            if (succeed)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = DateTime.Now,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = descriptionNeeded ? Modules.Log.Action.SetWorkFlowStateDescriptionAsNeeded :
                        Modules.Log.Action.SetWorkFlowStateDescriptionAsNotNeeded,
                    SubjectID = workflowId,
                    SecondSubjectID = stateId,
                    ModuleIdentifier = ModuleIdentifier.WF
                });
            }
            //end of Save Log
        }

        protected void set_state_hide_owner_name(Guid workflowId, Guid stateId, bool hideOwnerName, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!AuthorizationManager.has_right(AccessRoleName.ManageWorkflow, paramsContainer.CurrentUserID))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                _save_error_log(hideOwnerName ? Modules.Log.Action.SetWorkFlowStateHideOwnerNameAsNeeded_PermissionFailed :
                        Modules.Log.Action.SetWorkFlowStateHideOwnerNameAsNotNeeded_PermissionFailed, workflowId, stateId);
                return;
            }

            bool succeed = WFController.set_state_hide_owner_name(paramsContainer.Tenant.Id, workflowId,
                stateId, hideOwnerName, paramsContainer.CurrentUserID.Value);

            responseText = succeed ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";

            //Save Log
            if (succeed)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = DateTime.Now,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = hideOwnerName ? Modules.Log.Action.SetWorkFlowStateHideOwnerNameAsNeeded :
                        Modules.Log.Action.SetWorkFlowStateHideOwnerNameAsNotNeeded,
                    SubjectID = workflowId,
                    SecondSubjectID = stateId,
                    ModuleIdentifier = ModuleIdentifier.WF
                });
            }
            //end of Save Log
        }

        protected void set_state_edit_permission(Guid workflowId, Guid stateId, bool editPermission, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!AuthorizationManager.has_right(AccessRoleName.ManageWorkflow, paramsContainer.CurrentUserID))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                _save_error_log(Modules.Log.Action.SetWorkFlowStateEditPermission_PermissionFailed, workflowId, stateId);
                return;
            }

            bool succeed = WFController.set_state_edit_permisison(paramsContainer.Tenant.Id, workflowId,
                stateId, editPermission, paramsContainer.CurrentUserID.Value);

            responseText = succeed ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";

            //Save Log
            if (succeed)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = DateTime.Now,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.SetWorkFlowStateEditPermission,
                    SubjectID = workflowId,
                    SecondSubjectID = stateId,
                    Info = "{\"EditPermission\":" + editPermission.ToString().ToLower() + "}",
                    ModuleIdentifier = ModuleIdentifier.WF
                });
            }
            //end of Save Log
        }

        protected void set_free_data_need_requests(Guid workflowId,
            Guid stateId, bool freeDataNeedRequests, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!AuthorizationManager.has_right(AccessRoleName.ManageWorkflow, paramsContainer.CurrentUserID))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                _save_error_log(freeDataNeedRequests ? Modules.Log.Action.SetDataNeedRequestsAsFree_PermissionFailed :
                        Modules.Log.Action.SetDataNeedRequestsAsNotFree_PermissionFailed, workflowId, stateId);
                return;
            }

            bool succeed = WFController.set_free_data_need_requests(paramsContainer.Tenant.Id, workflowId,
                stateId, freeDataNeedRequests, paramsContainer.CurrentUserID.Value);

            responseText = succeed ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";

            //Save Log
            if (succeed)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = DateTime.Now,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = freeDataNeedRequests ? Modules.Log.Action.SetDataNeedRequestsAsFree :
                        Modules.Log.Action.SetDataNeedRequestsAsNotFree,
                    SubjectID = workflowId,
                    SecondSubjectID = stateId,
                    ModuleIdentifier = ModuleIdentifier.WF
                });
            }
            //end of Save Log
        }

        protected void set_state_data_need(Guid workflowId, Guid stateId, Guid nodeTypeId,
            Guid previousNodeTypeId, Guid formId, string description, bool multiselect,
            bool admin, bool necessary, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!AuthorizationManager.has_right(AccessRoleName.ManageWorkflow, paramsContainer.CurrentUserID))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                _save_error_log(Modules.Log.Action.SetStateDataNeed_PermissionFailed, workflowId, stateId);
                return;
            }

            StateDataNeed need = new StateDataNeed()
            {
                ID = Guid.NewGuid(),
                WorkFlowID = workflowId,
                StateID = stateId,
                FormID = formId,
                Description = description,
                MultiSelect = multiselect,
                Admin = admin,
                Necessary = necessary,
                CreatorUserID = paramsContainer.CurrentUserID,
                CreationDate = DateTime.Now
            };

            need.DirectorNodeType.NodeTypeID = nodeTypeId;

            bool succeed = WFController.set_state_data_need(paramsContainer.Tenant.Id, need, previousNodeTypeId);

            responseText = succeed ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\",\"ID\":\"" + need.ID.ToString() + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";

            //Save Log
            if (succeed)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = need.CreationDate,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.SetStateDataNeed,
                    SubjectID = workflowId,
                    SecondSubjectID = stateId,
                    ThirdSubjectID = nodeTypeId,
                    Info = "{\"PreviousNodeTypeID\":\"" + (previousNodeTypeId == Guid.Empty ? string.Empty : previousNodeTypeId.ToString()) + "\"" +
                        ",\"FormID\":\"" + (formId == Guid.Empty ? string.Empty : formId.ToString()) + "\"" +
                        ",\"Description\":\"" + Base64.encode(description) + "\"" +
                        ",\"Multiselect\":" + multiselect.ToString().ToLower() +
                        ",\"Necessary\":" + necessary.ToString().ToLower() +
                        ",\"Admin\":" + admin.ToString().ToLower() + "}",
                    ModuleIdentifier = ModuleIdentifier.WF
                });
            }
            //end of Save Log
        }

        protected void remove_state_data_need(Guid workflowId, Guid stateId, Guid nodeTypeId, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!AuthorizationManager.has_right(AccessRoleName.ManageWorkflow, paramsContainer.CurrentUserID))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                _save_error_log(Modules.Log.Action.RemoveStateDataNeed_PermissionFailed, workflowId, stateId);
                return;
            }

            StateDataNeed need = new StateDataNeed()
            {
                WorkFlowID = workflowId,
                StateID = stateId,
                LastModifierUserID = paramsContainer.CurrentUserID,
                LastModificationDate = DateTime.Now
            };

            need.DirectorNodeType.NodeTypeID = nodeTypeId;

            bool succeed = WFController.remove_state_data_need(paramsContainer.Tenant.Id, need);

            responseText = succeed ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";

            //Save Log
            if (succeed)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = need.CreationDate,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.RemoveStateDataNeed,
                    SubjectID = workflowId,
                    SecondSubjectID = stateId,
                    ThirdSubjectID = nodeTypeId,
                    ModuleIdentifier = ModuleIdentifier.WF
                });
            }
            //end of Save Log
        }

        protected void set_rejection_settings(Guid workflowId, Guid stateId, int maxAllowedRejections,
            string rejectionTitle, Guid rejectionRefStateId, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!string.IsNullOrEmpty(rejectionTitle) && rejectionTitle.Length > 250)
            {
                responseText = "{\"ErrorText\":\"" + Messages.MaxAllowedInputLengthExceeded + "\"}";
                return;
            }
            else if (!PublicMethods.is_secure_title(rejectionTitle))
            {
                responseText = "{\"ErrorText\":\"" + Messages.TheTextIsFormattedBadly + "\"}";
                return;
            }

            if (!AuthorizationManager.has_right(AccessRoleName.ManageWorkflow, paramsContainer.CurrentUserID))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                _save_error_log(Modules.Log.Action.SetRejectionSettings_PermissionFailed, workflowId, stateId);
                return;
            }

            State st = new State()
            {
                WorkFlowID = workflowId,
                StateID = stateId,
                MaxAllowedRejections = maxAllowedRejections,
                RejectionTitle = rejectionTitle,
                RejectionRefStateID = rejectionRefStateId,
                LastModifierUserID = paramsContainer.CurrentUserID,
                LastModificationDate = DateTime.Now
            };


            bool succeed = WFController.set_rejection_settings(paramsContainer.Tenant.Id, st);

            responseText = succeed ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";

            //Save Log
            if (succeed)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = st.LastModificationDate,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.SetRejectionSettings,
                    SubjectID = workflowId,
                    SecondSubjectID = stateId,
                    Info = "{\"MaxAllowedRejections\":\"" + maxAllowedRejections.ToString() +
                        "\",\"RejectionTitle\":\"" + Base64.encode(rejectionTitle) +
                        "\",\"RejectionRefStateID\":\"" + (rejectionRefStateId == Guid.Empty ? string.Empty : rejectionRefStateId.ToString()) +
                        "\"}",
                    ModuleIdentifier = ModuleIdentifier.WF
                });
            }
            //end of Save Log
        }

        protected void set_max_allowed_rejections(Guid workflowId,
            Guid stateId, int maxAllowedRejections, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!AuthorizationManager.has_right(AccessRoleName.ManageWorkflow, paramsContainer.CurrentUserID))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                _save_error_log(Modules.Log.Action.SetMaxAllowedRejections_PermissionFailed, workflowId, stateId);
                return;
            }

            State st = new State()
            {
                WorkFlowID = workflowId,
                StateID = stateId,
                MaxAllowedRejections = maxAllowedRejections,
                LastModifierUserID = paramsContainer.CurrentUserID,
                LastModificationDate = DateTime.Now
            };


            bool succeed = WFController.set_max_allowed_rejections(paramsContainer.Tenant.Id, st);

            responseText = succeed ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";

            //Save Log
            if (succeed)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = st.LastModificationDate,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.SetMaxAllowedRejections,
                    SubjectID = workflowId,
                    SecondSubjectID = stateId,
                    Info = "{\"MaxAllowedRejections\":\"" + maxAllowedRejections.ToString() + "\"}",
                    ModuleIdentifier = ModuleIdentifier.WF
                });
            }
            //end of Save Log
        }

        protected void create_state_data_need_instance(Guid historyId,
            Guid nodeId, bool admin, Guid formId, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!_has_workflow_permission(WFController.get_history_owner_id(paramsContainer.Tenant.Id, historyId)))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                return;
            }

            StateDataNeedInstance instance = new StateDataNeedInstance()
            {
                InstanceID = Guid.NewGuid(),
                HistoryID = historyId,
                Admin = admin,
                FormID = formId,
                CreatorUserID = paramsContainer.CurrentUserID,
                CreationDate = DateTime.Now
            };

            instance.DirectorNode.NodeID = nodeId;

            List<Dashboard> sentDashboards = new List<Dashboard>();
            string errorMessage = string.Empty;

            bool succeed = WFController.create_state_data_need_instance(paramsContainer.Tenant.Id,
                instance, ref sentDashboards, ref errorMessage);

            responseText = !succeed ? "{\"ErrorText\":\"" + (string.IsNullOrEmpty(errorMessage) ? Messages.OperationFailed.ToString() : errorMessage) + "\"}" :
                "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\",\"InstanceID\":\"" + instance.InstanceID.Value.ToString() + "\"}";

            //Send Notification
            if (succeed && sentDashboards.Count > 0)
                NotificationController.transfer_dashboards(paramsContainer.Tenant.Id, sentDashboards);
            //end of Send Notification
        }

        protected void get_state_data_need_instance(Guid? instanceId, ref string responseText)
        {
            if (!paramsContainer.GBEdit) return;

            StateDataNeedInstance instance = !instanceId.HasValue ? null :
                WFController.get_state_data_need_instance(paramsContainer.Tenant.Id, instanceId.Value);

            if (instance == null)
            {
                responseText = "{\"ErrorText\":\"" + "داده تکمیلی یافت نشد" + "\"}";
                return;
            }

            History history = WFController.get_history(paramsContainer.Tenant.Id, instance.HistoryID.Value);
            Node owner = CNController.get_node(paramsContainer.Tenant.Id, history.OwnerID.Value);
            StateDataNeed dataNeed = WFController.get_state_data_need(paramsContainer.Tenant.Id, history.WorkFlowID.Value,
                history.State.StateID.Value, instance.DirectorNode.NodeTypeID.Value);
            FormType form = FGController.get_owner_form_instances(paramsContainer.Tenant.Id, instanceId.Value).FirstOrDefault();
            if (form == null) form = new FormType();
            List<DocFileInfo> preAttachedFiles = DocumentsController.get_owner_files(paramsContainer.Tenant.Id, instanceId.Value);
            List<DocFileInfo> attachments = !instance.AttachmentID.HasValue ? new List<DocFileInfo>() :
                DocumentsController.get_owner_files(paramsContainer.Tenant.Id, instance.AttachmentID.Value);

            string nodeName = string.IsNullOrEmpty(owner.Name) ? string.Empty : Base64.encode(owner.Name);
            string nodeType = string.IsNullOrEmpty(owner.NodeType) ? string.Empty : Base64.encode(owner.NodeType);
            string description = string.IsNullOrEmpty(dataNeed.Description) ? string.Empty : Base64.encode(dataNeed.Description);
            string formTitle = string.IsNullOrEmpty(form.Title) ? string.Empty : Base64.encode(form.Title);

            responseText = "{\"InstanceID\":\"" + instance.InstanceID.Value.ToString() +
                "\",\"Filled\":" + (instance.Filled.HasValue ? instance.Filled.Value : false).ToString().ToLower() +
                ",\"FillingDate\":\"" + (instance.FillingDate.HasValue ?
                    GenericDate.get_local_date(instance.FillingDate.Value, true) : string.Empty) +
                "\",\"AttachmentID\":\"" + (!instance.AttachmentID.HasValue ? string.Empty : instance.AttachmentID.Value.ToString()) +
                "\",\"Necessary\":" + (!dataNeed.Necessary.HasValue ? "false" : dataNeed.Necessary.Value.ToString().ToLower()) +
                ",\"NodeID\":\"" + owner.NodeID.Value.ToString() + "\",\"NodeName\":\"" + nodeName +
                "\",\"NodeType\":\"" + nodeType + "\",\"Description\":\"" + description +
                "\",\"Form\":{\"FormID\":\"" + (form.FormID.HasValue ? form.FormID.ToString() : string.Empty) +
                "\",\"InstanceID\":\"" + (form.InstanceID.HasValue ? form.InstanceID.ToString() : string.Empty) +
                "\",\"Title\":\"" + formTitle +
                "\",\"Filled\":" + (form.Filled.HasValue ? form.Filled.Value : false).ToString().ToLower() +
                ",\"FillingDate\":\"" + (form.FillingDate.HasValue ? GenericDate.get_local_date(form.FillingDate.Value, true) : string.Empty) +
                "\"},\"PreAttachedFiles\":" + DocumentUtilities.get_files_json(preAttachedFiles) +
                ",\"Attachments\":" + DocumentUtilities.get_files_json(attachments) + "}";
        }

        protected void set_state_data_need_instance_as_filled(Guid? instanceId, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            StateDataNeedInstance inst = !instanceId.HasValue ? null : 
                WFController.get_state_data_need_instance(paramsContainer.Tenant.Id, instanceId.Value);

            if (inst == null || (
                !_has_workflow_permission(WFController.get_history_owner_id(paramsContainer.Tenant.Id, inst.HistoryID.Value)) &&
                !CNController.is_node_member(paramsContainer.Tenant.Id, inst.DirectorNode.NodeID.Value,
                paramsContainer.CurrentUserID.Value, inst.Admin, NodeMemberStatuses.Accepted)))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                _save_error_log(Modules.Log.Action.SetStateDataNeedInstanceAsFilled_PermissionFailed, instanceId);
                return;
            }

            bool succeed = instanceId.HasValue && 
                WFController.set_state_data_need_instance_as_filled(paramsContainer.Tenant.Id,
                instanceId.Value, paramsContainer.CurrentUserID.Value);

            responseText = succeed ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";

            //Save Log
            if (succeed)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = DateTime.Now,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.SetStateDataNeedInstanceAsFilled,
                    SubjectID = instanceId,
                    ModuleIdentifier = ModuleIdentifier.WF
                });
            }
            //end of Save Log
        }

        protected void set_state_data_need_instance_as_not_filled(Guid? instanceId, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            StateDataNeedInstance inst = !instanceId.HasValue ? null :
                WFController.get_state_data_need_instance(paramsContainer.Tenant.Id, instanceId.Value);

            if (inst == null ||
                !_has_workflow_permission(WFController.get_history_owner_id(paramsContainer.Tenant.Id, inst.HistoryID.Value)))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                _save_error_log(Modules.Log.Action.SetStateDataNeedInstanceAsNotFilled_PermissionFailed, instanceId);
                return;
            }

            List<Dashboard> sentDashboards = new List<Dashboard>();
            string errorText = string.Empty;

            bool succeed = instanceId.HasValue &&
                WFController.set_state_data_need_instance_as_not_filled(paramsContainer.Tenant.Id, instanceId.Value,
                paramsContainer.CurrentUserID.Value, ref sentDashboards, ref errorText);

            responseText = succeed ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + (string.IsNullOrEmpty(errorText) ? Messages.OperationFailed.ToString() : errorText) + "\"}";

            //Send Notification
            if (succeed && sentDashboards.Count > 0)
                NotificationController.transfer_dashboards(paramsContainer.Tenant.Id, sentDashboards);
            //end of Send Notification

            //Save Log
            if (succeed)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = DateTime.Now,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.SetStateDataNeedInstanceAsNotFilled,
                    SubjectID = instanceId,
                    ModuleIdentifier = ModuleIdentifier.WF
                });
            }
            //end of Save Log
        }

        protected void remove_state_data_need_instance(Guid instanceId, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            StateDataNeedInstance inst = WFController.get_state_data_need_instance(paramsContainer.Tenant.Id, instanceId);

            if (!_has_workflow_permission(WFController.get_history_owner_id(paramsContainer.Tenant.Id, inst.HistoryID.Value)))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                _save_error_log(Modules.Log.Action.RemoveStateDataNeedInstance_PermissionFailed, instanceId);
                return;
            }

            bool succeed = WFController.remove_state_data_need_instance(paramsContainer.Tenant.Id,
                instanceId, paramsContainer.CurrentUserID.Value);

            responseText = succeed ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";

            //Save Log
            if (succeed)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = DateTime.Now,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.RemoveStateDataNeedInstance,
                    SubjectID = instanceId,
                    ModuleIdentifier = ModuleIdentifier.WF
                });
            }
            //end of Save Log
        }

        protected void add_state_connection(Guid workflowId, Guid inStateId, Guid outStateId, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!AuthorizationManager.has_right(AccessRoleName.ManageWorkflow, paramsContainer.CurrentUserID))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                _save_error_log(Modules.Log.Action.AddStateConnection_PermissionFailed, workflowId, inStateId, outStateId);
                return;
            }

            StateConnection connection = new StateConnection()
            {
                WorkFlowID = workflowId,
                CreatorUserID = paramsContainer.CurrentUserID,
                CreationDate = DateTime.Now
            };

            connection.InState.StateID = inStateId;
            connection.OutState.StateID = outStateId;

            Guid? id = WFController.add_state_connection(paramsContainer.Tenant.Id, connection);
            bool succeed = id.HasValue;

            responseText = !succeed ? "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}" :
                "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"" + ",\"ID\":\"" + id.ToString() + "\"}";

            //Save Log
            if (succeed)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = connection.CreationDate,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.AddStateConnection,
                    SubjectID = workflowId,
                    SecondSubjectID = inStateId,
                    ThirdSubjectID = outStateId,
                    ModuleIdentifier = ModuleIdentifier.WF
                });
            }
            //end of Save Log
        }

        protected void sort_state_connections(List<Guid> ids, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!AuthorizationManager.has_right(AccessRoleName.ManageWorkflow, paramsContainer.CurrentUserID))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                return;
            }

            bool succeed = WFController.sort_state_connections(paramsContainer.Tenant.Id, ids);

            responseText = succeed ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";
        }

        protected void move_state_connection(Guid workflowId,
            Guid inStateId, Guid outStateId, bool moveDown, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!AuthorizationManager.has_right(AccessRoleName.ManageWorkflow, paramsContainer.CurrentUserID))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                _save_error_log(moveDown ? Modules.Log.Action.MoveStateConnectionDown_PermissionFailed :
                    Modules.Log.Action.MoveStateConnectionUp_PermissionFailed, workflowId, inStateId, outStateId);
                return;
            }

            StateConnection connection = new StateConnection();

            connection.WorkFlowID = workflowId;
            connection.InState.StateID = inStateId;
            connection.OutState.StateID = outStateId;

            bool succeed = WFController.move_state_connection(paramsContainer.Tenant.Id, connection, moveDown);

            responseText = succeed ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";

            //Save Log
            if (succeed)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = DateTime.Now,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = moveDown ? Modules.Log.Action.MoveStateConnectionDown : Modules.Log.Action.MoveStateConnectionUp,
                    SubjectID = workflowId,
                    SecondSubjectID = inStateId,
                    ThirdSubjectID = outStateId,
                    ModuleIdentifier = ModuleIdentifier.WF
                });
            }
            //end of Save Log
        }

        protected void remove_state_connection(Guid workflowId, Guid inStateId, Guid outStateId, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!AuthorizationManager.has_right(AccessRoleName.ManageWorkflow, paramsContainer.CurrentUserID))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                _save_error_log(Modules.Log.Action.RemoveStateConnection_PermissionFailed, workflowId, inStateId, outStateId);
                return;
            }

            StateConnection connection = new StateConnection()
            {
                WorkFlowID = workflowId,
                LastModifierUserID = paramsContainer.CurrentUserID,
                LastModificationDate = DateTime.Now
            };

            connection.InState.StateID = inStateId;
            connection.OutState.StateID = outStateId;

            bool succeed = WFController.remove_state_connection(paramsContainer.Tenant.Id, connection);

            responseText = succeed ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";

            //Save Log
            if (succeed)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = connection.LastModificationDate,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.RemoveStateConnection,
                    SubjectID = workflowId,
                    SecondSubjectID = inStateId,
                    ThirdSubjectID = outStateId,
                    ModuleIdentifier = ModuleIdentifier.WF
                });
            }
            //end of Save Log
        }

        protected void set_state_connection_label(Guid workflowId, Guid inStateId, Guid outStateId,
            string label, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!string.IsNullOrEmpty(label) && label.Length > 250)
            {
                responseText = "{\"ErrorText\":\"" + Messages.MaxAllowedInputLengthExceeded + "\"}";
                return;
            }
            else if (!PublicMethods.is_secure_title(label))
            {
                responseText = "{\"ErrorText\":\"" + Messages.TheTextIsFormattedBadly + "\"}";
                return;
            }

            if (!AuthorizationManager.has_right(AccessRoleName.ManageWorkflow, paramsContainer.CurrentUserID))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                _save_error_log(Modules.Log.Action.SetStateConnectionLabel_PermissionFailed, workflowId, inStateId, outStateId);
                return;
            }

            StateConnection connection = new StateConnection()
            {
                WorkFlowID = workflowId,
                Label = label,
                LastModifierUserID = paramsContainer.CurrentUserID,
                LastModificationDate = DateTime.Now
            };

            connection.InState.StateID = inStateId;
            connection.OutState.StateID = outStateId;

            bool succeed = WFController.set_state_connection_label(paramsContainer.Tenant.Id, connection);

            responseText = succeed ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";

            //Save Log
            if (succeed)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = connection.LastModificationDate,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.SetStateConnectionLabel,
                    SubjectID = workflowId,
                    SecondSubjectID = inStateId,
                    ThirdSubjectID = outStateId,
                    Info = "{\"Label\":\"" + Base64.encode(label) + "\"}",
                    ModuleIdentifier = ModuleIdentifier.WF
                });
            }
            //end of Save Log
        }

        protected void set_state_connection_attachment_status(Guid workflowId, Guid inStateId, Guid outStateId,
            bool attachmentRequired, string attachmentTitle, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!string.IsNullOrEmpty(attachmentTitle) && attachmentTitle.Length > 250)
            {
                responseText = "{\"ErrorText\":\"" + Messages.MaxAllowedInputLengthExceeded + "\"}";
                return;
            }

            if (!AuthorizationManager.has_right(AccessRoleName.ManageWorkflow, paramsContainer.CurrentUserID))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                _save_error_log(Modules.Log.Action.SetStateConnectionAttachmentStatus_PermissionFailed,
                    workflowId, inStateId, outStateId);
                return;
            }

            StateConnection connection = new StateConnection()
            {
                WorkFlowID = workflowId,
                AttachmentRequired = attachmentRequired,
                AttachmentTitle = attachmentTitle,
                LastModifierUserID = paramsContainer.CurrentUserID,
                LastModificationDate = DateTime.Now
            };

            connection.InState.StateID = inStateId;
            connection.OutState.StateID = outStateId;

            bool succeed = WFController.set_state_connection_attachment_status(paramsContainer.Tenant.Id, connection);

            responseText = succeed ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";

            //Save Log
            if (succeed)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = connection.LastModificationDate,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.SetStateConnectionAttachmentStatus,
                    SubjectID = workflowId,
                    SecondSubjectID = inStateId,
                    ThirdSubjectID = outStateId,
                    Info = "{\"AttachmentTitle\":\"" + Base64.encode(attachmentTitle) +
                        "\",\"AttachmentRequired\":" + attachmentRequired.ToString().ToLower() + "}",
                    ModuleIdentifier = ModuleIdentifier.WF
                });
            }
            //end of Save Log
        }

        protected void set_state_connection_director(Guid workflowId, Guid inStateId, Guid outStateId,
            bool nodeRequired, Guid nodeTypeId, string description, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!string.IsNullOrEmpty(description) && description.Length > 1900)
            {
                responseText = "{\"ErrorText\":\"" + Messages.MaxAllowedInputLengthExceeded + "\"}";
                return;
            }

            if (!AuthorizationManager.has_right(AccessRoleName.ManageWorkflow, paramsContainer.CurrentUserID))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                _save_error_log(Modules.Log.Action.SetStateConnectionDirector_PermissionFailed,
                    workflowId, inStateId, outStateId);
                return;
            }

            StateConnection connection = new StateConnection()
            {
                WorkFlowID = workflowId,
                NodeRequired = nodeRequired,
                LastModifierUserID = paramsContainer.CurrentUserID,
                LastModificationDate = DateTime.Now,
                NodeTypeDescription = description
            };

            connection.InState.StateID = inStateId;
            connection.OutState.StateID = outStateId;
            if (nodeTypeId != Guid.Empty) connection.DirectorNodeType.NodeTypeID = nodeTypeId;

            bool succeed = WFController.set_state_connection_director(paramsContainer.Tenant.Id, connection);

            responseText = succeed ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";

            //Save Log
            if (succeed)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = connection.LastModificationDate,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.SetStateConnectionDirector,
                    SubjectID = workflowId,
                    SecondSubjectID = inStateId,
                    ThirdSubjectID = outStateId,
                    Info = "{\"Description\":\"" + Base64.encode(description) +
                        "\",\"NodeTypeID\":\"" + (nodeTypeId == Guid.Empty ? string.Empty : nodeTypeId.ToString()) +
                        "\",\"NodeRequired\":" + nodeRequired.ToString().ToLower() + "}",
                    ModuleIdentifier = ModuleIdentifier.WF
                });
            }
            //end of Save Log
        }

        protected void set_state_connection_form(Guid workflowId, Guid inStateId, Guid outStateId,
            Guid formId, string description, bool necessary, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!string.IsNullOrEmpty(description) && description.Length > 3900)
            {
                responseText = "{\"ErrorText\":\"" + Messages.MaxAllowedInputLengthExceeded + "\"}";
                return;
            }

            if (!AuthorizationManager.has_right(AccessRoleName.ManageWorkflow, paramsContainer.CurrentUserID))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                _save_error_log(Modules.Log.Action.SetStateConnectionForm_PermissionFailed,
                    workflowId, inStateId, outStateId);
                return;
            }

            StateConnectionForm form = new StateConnectionForm()
            {
                WorkFlowID = workflowId,
                InStateID = inStateId,
                OutStateID = outStateId,
                Description = description,
                Necessary = necessary,
                CreatorUserID = paramsContainer.CurrentUserID,
                CreationDate = DateTime.Now
            };

            form.Form.FormID = formId;

            bool succeed = WFController.set_state_connection_form(paramsContainer.Tenant.Id, form);

            responseText = succeed ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";

            //Save Log
            if (succeed)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = form.CreationDate,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.SetStateConnectionForm,
                    SubjectID = workflowId,
                    SecondSubjectID = inStateId,
                    ThirdSubjectID = outStateId,
                    FourthSubjectID = formId,
                    Info = "{\"Description\":\"" + Base64.encode(description) + "\",\"Necessary\":" + necessary.ToString().ToLower() + "}",
                    ModuleIdentifier = ModuleIdentifier.WF
                });
            }
            //end of Save Log
        }

        protected void remove_state_connection_form(Guid workflowId, Guid inStateId, Guid outStateId,
            Guid formId, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!AuthorizationManager.has_right(AccessRoleName.ManageWorkflow, paramsContainer.CurrentUserID))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                _save_error_log(Modules.Log.Action.RemoveStateConnectionForm_PermissionFailed,
                    workflowId, inStateId, outStateId);
                return;
            }

            StateConnectionForm form = new StateConnectionForm()
            {
                WorkFlowID = workflowId,
                InStateID = inStateId,
                OutStateID = outStateId,
                LastModifierUserID = paramsContainer.CurrentUserID,
                LastModificationDate = DateTime.Now
            };

            form.Form.FormID = formId;

            bool succeed = WFController.remove_state_connection_form(paramsContainer.Tenant.Id, form);

            responseText = succeed ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";

            //Save Log
            if (succeed)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = form.LastModificationDate,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.RemoveStateConnectionForm,
                    SubjectID = workflowId,
                    SecondSubjectID = inStateId,
                    ThirdSubjectID = outStateId,
                    FourthSubjectID = formId,
                    ModuleIdentifier = ModuleIdentifier.WF
                });
            }
            //end of Save Log
        }

        protected void add_auto_message(Guid? ownerId, string bodyText, AudienceTypes audienceType, Guid refStateId,
            Guid nodeId, bool admin, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!string.IsNullOrEmpty(bodyText) && bodyText.Length > 3900)
            {
                responseText = "{\"ErrorText\":\"" + Messages.MaxAllowedInputLengthExceeded + "\"}";
                return;
            }
            else if (!PublicMethods.is_secure_title(bodyText))
            {
                responseText = "{\"ErrorText\":\"" + Messages.TheTextIsFormattedBadly + "\"}";
                return;
            }

            if (!AuthorizationManager.has_right(AccessRoleName.ManageWorkflow, paramsContainer.CurrentUserID))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                _save_error_log(Modules.Log.Action.AddAutoMessage_PermissionFailed, ownerId);
                return;
            }

            AutoMessage automessage = new AutoMessage()
            {
                AutoMessageID = Guid.NewGuid(),
                OwnerID = ownerId,
                BodyText = bodyText,
                AudienceType = audienceType,
                Admin = admin,
                CreatorUserID = paramsContainer.CurrentUserID,
                CreationDate = DateTime.Now
            };

            automessage.RefState.StateID = refStateId;

            automessage.Node.NodeID = nodeId;

            bool succeed = WFController.add_auto_message(paramsContainer.Tenant.Id, automessage);

            responseText = succeed ?
                "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\",\"AutoMessage\":" +
                    _get_auto_message_json(automessage) + "}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";

            //Save Log
            if (succeed)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = automessage.CreationDate,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.AddAutoMessage,
                    SubjectID = automessage.AutoMessageID,
                    SecondSubjectID = ownerId,
                    Info = "{\"Description\":\"" + Base64.encode(bodyText) +
                        "\",\"AudienceType\":\"" + audienceType.ToString() +
                        "\",\"RefStateID\":\"" + (refStateId == Guid.Empty ? string.Empty : refStateId.ToString()) +
                        "\",\"NodeID\":\"" + (nodeId == Guid.Empty ? string.Empty : nodeId.ToString()) +
                        "\",\"Admin\":" + admin.ToString().ToLower() + "}",
                    ModuleIdentifier = ModuleIdentifier.WF
                });
            }
            //end of Save Log
        }

        protected void modify_auto_message(Guid automessageId, string bodyText, AudienceTypes audienceType,
            Guid refStateId, Guid nodeId, bool admin, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!string.IsNullOrEmpty(bodyText) && bodyText.Length > 3900)
            {
                responseText = "{\"ErrorText\":\"" + Messages.MaxAllowedInputLengthExceeded + "\"}";
                return;
            }

            if (!AuthorizationManager.has_right(AccessRoleName.ManageWorkflow, paramsContainer.CurrentUserID))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                _save_error_log(Modules.Log.Action.ModifyAutoMessage_PermissionFailed, automessageId);
                return;
            }

            AutoMessage automessage = new AutoMessage()
            {
                AutoMessageID = automessageId,
                BodyText = bodyText,
                AudienceType = audienceType,
                Admin = admin,
                LastModifierUserID = paramsContainer.CurrentUserID,
                LastModificationDate = DateTime.Now
            };

            automessage.Node.NodeID = nodeId;
            automessage.RefState.StateID = refStateId;

            bool succeed = WFController.modify_auto_message(paramsContainer.Tenant.Id, automessage);

            responseText = succeed ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";

            //Save Log
            if (succeed)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = automessage.LastModificationDate,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.ModifyAutoMessage,
                    SubjectID = automessage.AutoMessageID,
                    Info = "{\"Description\":\"" + Base64.encode(bodyText) +
                        "\",\"AudienceType\":\"" + audienceType.ToString() +
                        "\",\"RefStateID\":\"" + (refStateId == Guid.Empty ? string.Empty : refStateId.ToString()) +
                        "\",\"NodeID\":\"" + (nodeId == Guid.Empty ? string.Empty : nodeId.ToString()) +
                        "\",\"Admin\":" + admin.ToString().ToLower() + "}",
                    ModuleIdentifier = ModuleIdentifier.WF
                });
            }
            //end of Save Log
        }

        protected void remove_auto_message(Guid automessageId, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!AuthorizationManager.has_right(AccessRoleName.ManageWorkflow, paramsContainer.CurrentUserID))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                _save_error_log(Modules.Log.Action.RemoveAutoMessage_PermissionFailed, automessageId);
                return;
            }

            bool succeed = WFController.remove_auto_message(paramsContainer.Tenant.Id,
                automessageId, paramsContainer.CurrentUserID.Value);

            responseText = succeed ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";

            //Save Log
            if (succeed)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = DateTime.Now,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.RemoveAutoMessage,
                    SubjectID = automessageId,
                    ModuleIdentifier = ModuleIdentifier.WF
                });
            }
            //end of Save Log
        }

        protected string _get_history_form_instance_json(HistoryFormInstance instance)
        {
            string retStr = string.Empty;

            bool isFirst = true;
            foreach (FormType frm in instance.Forms)
            {
                string title = Base64.encode(string.IsNullOrEmpty(frm.Title) ? string.Empty : frm.Title);
                string creatorFullName = Base64.encode(
                    (string.IsNullOrEmpty(frm.Creator.FirstName) ? string.Empty : frm.Creator.FirstName) + " " +
                    (string.IsNullOrEmpty(frm.Creator.LastName) ? string.Empty : frm.Creator.LastName));

                if (!isFirst) retStr += ",";
                isFirst = false;

                retStr += "{\"InstanceID\":\"" + (frm.InstanceID.HasValue ? frm.InstanceID.Value.ToString() : string.Empty) +
                    "\",\"Title\":\"" + title + "\",\"FormID\":\"" + (frm.FormID.HasValue ? frm.FormID.Value.ToString() : string.Empty) +
                    "\",\"Filled\":" + (frm.Filled.HasValue ? frm.Filled.Value : false).ToString().ToLower() +
                    ",\"FillingDate\":\"" + (frm.FillingDate.HasValue ?
                        GenericDate.get_local_date(frm.FillingDate.Value, true) : string.Empty) +
                        "\",\"Creator\":{\"UserID\":\"" + (frm.Creator.UserID.HasValue ? frm.Creator.UserID.Value.ToString() : string.Empty) +
                    "\",\"FullName\":\"" + creatorFullName +
                    "\",\"ProfileImageURL\":\"" + (frm.Creator.UserID.HasValue ?
                        DocumentUtilities.get_personal_image_address(paramsContainer.Tenant.Id, frm.Creator.UserID.Value) :
                        string.Empty) + "\"}}";

            }

            return retStr;
        }

        protected string _get_history_json(History history)
        {
            string stateTitle = Base64.encode(string.IsNullOrEmpty(history.State.Title) ? string.Empty : history.State.Title);
            string description = Base64.encode(string.IsNullOrEmpty(history.Description) ? string.Empty : history.Description);
            string directorFullName = Base64.encode(
                (string.IsNullOrEmpty(history.Sender.FirstName) ? string.Empty : history.Sender.FirstName) + " " +
                (string.IsNullOrEmpty(history.Sender.LastName) ? string.Empty : history.Sender.LastName));
            string directorNodeName = Base64.encode(string.IsNullOrEmpty(history.DirectorNode.Name) ?
                string.Empty : history.DirectorNode.Name);
            string directorNodeType = Base64.encode((string.IsNullOrEmpty(history.DirectorNode.NodeType) ?
                string.Empty : history.DirectorNode.NodeType));

            return "{\"HistoryID\":\"" + history.HistoryID.Value.ToString() + "\"" +
                ",\"PreviousHistoryID\":\"" + (history.PreviousHistoryID.HasValue ?
                    history.PreviousHistoryID.ToString() : string.Empty) + "\"" +
                ",\"OwnerID\":\"" + history.OwnerID.Value.ToString() + "\"" +
                ",\"WorkFlowID\":\"" + history.WorkFlowID.Value.ToString() + "\"" +
                ",\"StateID\":\"" + history.State.StateID.Value.ToString() + "\"" +
                ",\"StateTitle\":\"" + stateTitle + "\"" +
                ",\"Description\":\"" + description + "\"" +
                ",\"SendDate\":\"" + (history.SendDate.HasValue ?
                    GenericDate.get_local_date(history.SendDate.Value, true) : string.Empty) + "\"" +
                ",\"PollID\":\"" + (history.PollID.HasValue ? history.PollID.ToString() : string.Empty) + "\"" +
                ",\"PollName\":\"" + (history.PollID.HasValue ? Base64.encode(history.PollName) : string.Empty) + "\"" +
                (!history.PollID.HasValue ? string.Empty : ",\"PollID\":\"" + history.PollID.ToString() + "\"") +
                ",\"Director\":{\"UserID\":\"" + (history.Sender.UserID.HasValue ?
                        history.Sender.UserID.Value.ToString() : string.Empty) + "\"" +
                    ",\"FullName\":\"" + directorFullName + "\"" +
                    ",\"ProfileImageURL\":\"" + (history.Sender.UserID.HasValue ?
                        DocumentUtilities.get_personal_image_address(paramsContainer.Tenant.Id,
                        history.Sender.UserID.Value) : string.Empty) + "\"" +
                    ",\"NodeID\":\"" + (history.DirectorNode.NodeID.HasValue ?
                        history.DirectorNode.NodeID.Value.ToString() : string.Empty) + "\"" +
                    ",\"NodeName\":\"" + directorNodeName + "\"" +
                    ",\"NodeType\":\"" + directorNodeType + "\"" + "}" +
                ",\"AttachedFiles\":[" + string.Join(",",
                    history.AttachedFiles.Select(u => u.toJson())) + "]" +
                ",\"Forms\":[" + string.Join(",",
                    history.FormInstances.Select(x => _get_history_form_instance_json(x))) + "]" +
                "}";
        }

        protected void get_owner_history(Guid ownerId, bool lastOnly, bool done, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!_has_workflow_permission(ownerId))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                return;
            }

            List<History> history = new List<History>();

            if (lastOnly)
            {
                History hist = WFController.get_last_history(paramsContainer.Tenant.Id, ownerId, null, done);
                if (hist != null) history.Add(hist);
            }
            else history = WFController.get_owner_history(paramsContainer.Tenant.Id, ownerId);

            List<Guid> histIds = history.Where(u => u.HistoryID.HasValue).Select(u => u.HistoryID.Value).ToList();

            List<DocFileInfo> attachedFiles = DocumentsController.get_owner_files(paramsContainer.Tenant.Id, histIds);
            List<HistoryFormInstance> histFormInstances =
                WFController.get_history_form_instances(paramsContainer.Tenant.Id, histIds, true);

            List<Guid> formsIds = histFormInstances.Where(u => u.FormsID.HasValue).Select(u => u.FormsID.Value).ToList();

            List<FormType> formInstances = FGController.get_owner_form_instances(paramsContainer.Tenant.Id, formsIds);

            foreach (HistoryFormInstance inst in histFormInstances)
                inst.Forms = formInstances.Where(u => u.OwnerID == inst.FormsID).ToList();

            responseText = lastOnly ? string.Empty : "{\"States\":[";

            bool isFirst = true;
            for (int i = 0, lnt = history.Count; i < lnt; ++i)
            {
                History hist = history[i];

                if (!lastOnly &&
                    ((!hist.PreviousHistoryID.HasValue && i != lnt - 1) || !hist.Sender.UserID.HasValue)) continue;

                hist.FormInstances = histFormInstances.Where(u => u.HistoryID == hist.HistoryID).ToList();
                hist.AttachedFiles = attachedFiles.Where(u => u.OwnerID == hist.HistoryID).ToList();

                if (lastOnly)
                {
                    responseText = _get_history_json(hist);
                    return;
                }

                responseText += (isFirst ? string.Empty : ",") + _get_history_json(hist);
                isFirst = false;
            }

            responseText += lastOnly ? string.Empty : "]}";
        }

        protected string _get_history_connection_json(StateConnection connection)
        {
            string directorNodeType = connection.DirectorNodeType.Name;
            string nodeTypeDescription = connection.NodeTypeDescription;
            string label = connection.Label;
            string attachmentTitle = connection.AttachmentTitle;

            Base64.encode(directorNodeType, ref directorNodeType);
            Base64.encode(nodeTypeDescription, ref nodeTypeDescription);
            Base64.encode(label, ref label);
            Base64.encode(attachmentTitle, ref attachmentTitle);

            if (!connection.AttachmentRequired.HasValue) connection.AttachmentRequired = false;
            if (!connection.NodeRequired.HasValue) connection.NodeRequired = false;

            return "{\"DirectorNodeTypeID\":\"" + (connection.DirectorNodeType.NodeTypeID.HasValue ?
                connection.DirectorNodeType.NodeTypeID.Value.ToString() : string.Empty) +
                "\",\"DirectorNodeType\":\"" + directorNodeType + "\",\"NodeTypeDescription\":\"" + nodeTypeDescription +
                "\",\"OutStateID\":\"" + connection.OutState.StateID.Value.ToString() + "\",\"Label\":\"" + label +
                "\",\"NodeRequired\":" + connection.NodeRequired.Value.ToString().ToLower() +
                ",\"AttachmentRequired\":" + connection.AttachmentRequired.Value.ToString().ToLower() +
                ",\"AttachmentTitle\":\"" + attachmentTitle + "\"" +
                ",\"TemplateFiles\":[" + string.Join(",",
                    connection.AttachedFiles.Select(u => u.toJson(icon: true))) + "]" +
                ",\"Forms\":[" + string.Join(",",
                    connection.HistoryFormInstances.Select(x => _get_history_form_instance_json(x))) + "]" +
                "}";
        }

        protected string _get_state_data_need_instance_json(StateDataNeedInstance instance)
        {
            string nodeName = instance.DirectorNode.Name;

            Base64.encode(nodeName, ref nodeName);

            return "{\"InstanceID\":\"" + instance.InstanceID.Value.ToString() +
                "\",\"NodeID\":\"" + instance.DirectorNode.NodeID.Value.ToString() +
                "\",\"NodeName\":\"" + nodeName +
                "\",\"Filled\":" + (instance.Filled.HasValue ? instance.Filled.Value : false).ToString().ToLower() +
                ",\"FillingDate\":\"" + (instance.FillingDate.HasValue ?
                    GenericDate.get_local_date(instance.FillingDate.Value) : string.Empty) +
                    "\",\"PreAttachedFiles\":" + DocumentUtilities.get_files_json(instance.PreAttachedFiles) + "}";
        }

        protected string _get_history_state_data_need_json(StateDataNeed dataNeed)
        {
            FormType frm = FGController.get_owner_form(paramsContainer.Tenant.Id, dataNeed.ID.Value);
            if (frm == null) frm = new FormType();

            string nodeType = Base64.encode(dataNeed.DirectorNodeType.Name);

            string retVal = "{\"NodeTypeID\":\"" + dataNeed.DirectorNodeType.NodeTypeID.Value.ToString() + "\"" +
                ",\"NodeType\":\"" + nodeType + "\"" +
                ",\"MultiSelect\":" + (dataNeed.MultiSelect.HasValue ? dataNeed.MultiSelect.Value : false).ToString().ToLower() +
                ",\"Admin\":" + (dataNeed.Admin.HasValue ? dataNeed.Admin.Value : false).ToString().ToLower() +
                ",\"Necessary\":" + (dataNeed.Necessary.HasValue ? dataNeed.Necessary.Value : false).ToString().ToLower() +
                ",\"FormID\":\"" + (frm.FormID.HasValue ? frm.FormID.ToString() : string.Empty) + "\"" +
                ",\"FormTitle\":\"" + (string.IsNullOrEmpty(frm.Title) ? string.Empty : Base64.encode(frm.Title)) + "\"" +
                ",\"Instances\":[";

            bool isFirst = true;
            foreach (StateDataNeedInstance inst in dataNeed.Instances)
            {
                retVal += (isFirst ? string.Empty : ",") + _get_state_data_need_instance_json(inst);
                isFirst = false;
            }

            return retVal + "]}";
        }

        protected void get_workflow_options(Guid ownerId, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            /* //--> there no need for this part of code
            if (!_has_workflow_permission(ownerId))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                return;
            }
            */

            ViewerStatus viewerStatus = WFController.get_viewer_status(paramsContainer.Tenant.Id,
                paramsContainer.CurrentUserID.Value, ownerId);

            bool isSystemAdmin =
                PublicMethods.is_system_admin(paramsContainer.Tenant.Id, paramsContainer.CurrentUserID.Value);
            bool isAdmin = isSystemAdmin ||
                CNController.is_service_admin(paramsContainer.Tenant.Id, ownerId, paramsContainer.CurrentUserID.Value);
            bool isTerminated = WFController.is_terminated(paramsContainer.Tenant.Id, ownerId);

            if (!isAdmin && viewerStatus != ViewerStatus.Owner && isTerminated) viewerStatus = ViewerStatus.None;
            if (viewerStatus != ViewerStatus.NotInWorkFlow && isAdmin) viewerStatus = ViewerStatus.Director;

            if (viewerStatus == ViewerStatus.None)
            {
                responseText = "{\"ViewerStatus\":\"" + viewerStatus.ToString() + "\"}";
                return;
            }

            bool isDirector = viewerStatus == ViewerStatus.Director;
            bool isMember = viewerStatus == ViewerStatus.Director || viewerStatus == ViewerStatus.DirectorNodeMember;

            History history = WFController.get_last_history(paramsContainer.Tenant.Id, ownerId);

            bool optionsNotNeeded = viewerStatus == ViewerStatus.Owner || isTerminated;

            State state = optionsNotNeeded ? new State() : WFController.get_workflow_state(paramsContainer.Tenant.Id,
                history.WorkFlowID.Value, history.State.StateID.Value);
            if (state == null)
            {
                responseText = "{\"ErrorText\":\"" + Messages.WorkFlowStateNotFound + "\"}";
                return;
            }

            Guid? formInstanceIdToBeFilled = !state.ID.HasValue ? Guid.Empty :
                FGController.get_common_form_instance_ids(paramsContainer.Tenant.Id,
                state.ID.Value, ownerId, true).FirstOrDefault();
            if (formInstanceIdToBeFilled == Guid.Empty) formInstanceIdToBeFilled = null;

            List<StateDataNeed> dataNeeds = optionsNotNeeded ? new List<StateDataNeed>() :
                WFController.get_current_state_data_needs(paramsContainer.Tenant.Id,
                history.WorkFlowID.Value, history.State.StateID.Value);
            List<StateDataNeedInstance> dataNeedInstances = optionsNotNeeded ? new List<StateDataNeedInstance>() :
                WFController.get_state_data_need_instances(paramsContainer.Tenant.Id, history.HistoryID.Value);
            List<StateConnection> connections = (!isDirector || optionsNotNeeded) ? new List<StateConnection>() :
                WFController.get_workflow_connections(paramsContainer.Tenant.Id,
                history.WorkFlowID.Value, history.State.StateID.Value)/*.OrderBy(u => u.SequenceNumber)*/.ToList();
            long postsCount = optionsNotNeeded ? 0 :
                SharingController.get_posts_count(paramsContainer.Tenant.Id, history.HistoryID);

            List<DocFileInfo> historyAttachedFiles =
                DocumentsController.get_owner_files(paramsContainer.Tenant.Id, history.HistoryID.Value);

            List<Guid> _attIds = connections.Select(u => u.ID.Value).ToList();
            _attIds.AddRange(dataNeedInstances.Select(u => u.InstanceID.Value).ToList());
            List<DocFileInfo> attachedFiles = optionsNotNeeded ? new List<DocFileInfo>() :
                DocumentsController.get_owner_files(paramsContainer.Tenant.Id, _attIds);

            List<StateConnectionForm> connectionForms = !isDirector || isTerminated ? new List<StateConnectionForm>() :
                WFController.get_workflow_connection_forms(paramsContainer.Tenant.Id,
                history.WorkFlowID.Value, history.State.StateID.Value);
            List<HistoryFormInstance> formInstances = !isDirector || isTerminated ? new List<HistoryFormInstance>() :
                WFController.get_history_form_instances(paramsContainer.Tenant.Id, history.HistoryID.Value, null);

            List<FormType> forms = optionsNotNeeded ? new List<FormType>() :
                FGController.get_owner_form_instances(paramsContainer.Tenant.Id,
                    formInstances.Select(u => u.FormsID.Value).ToList());

            foreach (HistoryFormInstance inst in formInstances)
                inst.Forms = forms.Where(u => u.OwnerID == inst.FormsID).ToList();

            foreach (StateConnectionForm frm in connectionForms)
            {
                if (!formInstances.Exists(u => u.OutStateID == frm.OutStateID))
                {
                    HistoryFormInstance inst = new HistoryFormInstance()
                    {
                        HistoryID = history.HistoryID,
                        OutStateID = frm.OutStateID
                    };

                    inst.Forms.Add(new FormType()
                    {
                        FormID = frm.Form.FormID,
                        Title = frm.Form.Title,
                        Filled = false
                    });

                    formInstances.Add(inst);
                }
                else
                {
                    foreach (HistoryFormInstance inst in formInstances)
                    {
                        if (inst.OutStateID == frm.OutStateID)
                        {
                            if (inst.Forms.Exists(u => u.FormID == frm.Form.FormID)) break;

                            inst.Forms.Add(new FormType()
                            {
                                FormID = frm.Form.FormID,
                                Title = frm.Form.Title,
                                Filled = false
                            });
                        }

                        break;
                    }
                }
            }

            foreach (StateConnection conn in connections)
            {
                conn.AttachedFiles = attachedFiles.Where(u => u.OwnerID == conn.ID).ToList();
                conn.HistoryFormInstances = formInstances.Where(u => u.OutStateID == conn.OutState.StateID).ToList();
            }

            foreach (StateDataNeedInstance sdni in dataNeedInstances)
                sdni.PreAttachedFiles = attachedFiles.Where(u => u.OwnerID == sdni.InstanceID).ToList();

            foreach (StateDataNeed need in dataNeeds) need.Instances = dataNeedInstances.Where(
                u => u.DirectorNode.NodeTypeID == need.DirectorNodeType.NodeTypeID).ToList();

            string stateTitle = Base64.encode(history.State.Title);
            string description = Base64.encode(state.Description);
            string dataNeedsDescription = Base64.encode(state.DataNeedsDescription);

            if (!state.DescriptionNeeded.HasValue) state.DescriptionNeeded = true;
            if (!state.HideOwnerName.HasValue) state.HideOwnerName = false;
            if (!state.EditPermission.HasValue) state.EditPermission = false;

            List<NodeCreator> ownerUsers = new List<NodeCreator>();
            Modules.CoreNetwork.Node ownerNode = null;
            if (!state.HideOwnerName.HasValue || !state.HideOwnerName.Value)
            {
                ownerUsers = CNController.get_node_creators(paramsContainer.Tenant.Id, ownerId, true);
                ownerNode = CNController.get_node(paramsContainer.Tenant.Id, ownerId, true);
            }

            string creatorUserId = ownerNode == null ? string.Empty : ownerNode.Creator.UserID.Value.ToString();
            string creatorUserName = ownerNode == null ? string.Empty : Base64.encode(ownerNode.Creator.UserName);
            string creatorFirstName = ownerNode == null ? string.Empty : Base64.encode(ownerNode.Creator.FirstName);
            string creatorLastName = ownerNode == null ? string.Empty : Base64.encode(ownerNode.Creator.LastName);
            string imageUrl = ownerNode == null ? string.Empty :
                DocumentUtilities.get_personal_image_address(paramsContainer.Tenant.Id, Guid.Parse(creatorUserId));

            string creator = ownerNode == null ? "null" :
                "{\"UserID\":\"" + creatorUserId + "\",\"UserName\":\"" + creatorUserName +
                "\",\"FirstName\":\"" + creatorFirstName + "\",\"LastName\":\"" + creatorLastName +
                "\",\"ProfileImageURL\":\"" + imageUrl + "\"}";

            bool rejectable = state.MaxAllowedRejections.HasValue && state.MaxAllowedRejections > 0 &&
                state.MaxAllowedRejections > WFController.get_rejections_count(paramsContainer.Tenant.Id,
                ownerId, state.WorkFlowID.Value, state.StateID.Value);

            bool finishable = isSystemAdmin || (isDirector && connections.Count == 0);

            string strPoll = string.Empty;

            if (state.PollID.HasValue)
            {
                int audienceCount = history.DirectorNode == null || !history.DirectorNode.NodeID.HasValue ? 1 :
                    CNController.get_members_count(paramsContainer.Tenant.Id, history.DirectorNode.NodeID.Value);

                strPoll = ",\"Poll\":{\"RefPollID\":\"" + state.PollID.ToString() + "\"" +
                    ",\"PollID\":\"" + (!history.PollID.HasValue ? string.Empty : history.PollID.ToString()) + "\"" +
                    ",\"Name\":\"" + Base64.encode(state.PollName) + "\"" +
                    ",\"AudienceCount\":" + audienceCount.ToString() + "}";
            }

            responseText = "{\"HistoryID\":\"" + history.HistoryID.Value.ToString() + "\"" +
                ",\"WorkFlowStateID\":\"" + (state.ID.HasValue ? state.ID.ToString() : string.Empty) + "\"" +
                ",\"HasHistory\":" + history.PreviousHistoryID.HasValue.ToString().ToLower() +
                ",\"Finishable\":" + finishable.ToString().ToLower() +
                ",\"IsTerminated\":" + isTerminated.ToString().ToLower() +
                ",\"FormInstanceIDToBeFilled\":\"" +
                    (formInstanceIdToBeFilled.HasValue ? formInstanceIdToBeFilled.ToString() : string.Empty) + "\"" +
                ",\"Description\":\"" + description + "\"" +
                ",\"DataNeedsDescription\":\"" + dataNeedsDescription + "\"" +
                ",\"IsAdmin\":" + isAdmin.ToString().ToLower() +
                ",\"ViewerStatus\":\"" + viewerStatus.ToString() + "\"" +
                ",\"DescriptionNeeded\":" + state.DescriptionNeeded.Value.ToString().ToLower() +
                ",\"StateTitle\":\"" + stateTitle + "\"" +
                ",\"PostsCount\":\"" + postsCount.ToString() + "\"" +
                ",\"Rejectable\":" + rejectable.ToString().ToLower() +
                ",\"RejectionTitle\":\"" + Base64.encode(state.RejectionTitle) + "\"" +
                strPoll +
                ",\"Attachments\":[" +
                    string.Join(",", historyAttachedFiles.Select(u => u.toJson(icon: true))) + "]" +
                ",\"Creator\":" + creator +
                ",\"Owners\":[" + string.Join(",", ownerUsers.Select(
                    nc => "{\"UserID\":\"" + nc.User.UserID.Value.ToString() + "\"" +
                        ",\"UserName\":\"" + Base64.encode(nc.User.UserName) + "\"" +
                        ",\"FirstName\":\"" + Base64.encode(nc.User.FirstName) + "\"" +
                        ",\"LastName\":\"" + Base64.encode(nc.User.LastName) + "\"" +
                        ",\"CollaborationShare\":\"" +
                            (nc.CollaborationShare.HasValue ? nc.CollaborationShare.ToString() : string.Empty) + "\"" +
                        ",\"ProfileImageURL\":\"" +
                            DocumentUtilities.get_personal_image_address(paramsContainer.Tenant.Id, nc.User.UserID.Value) + "\"" +
                        "}"
                )) + "]" +
                ",\"DataNeeds\":[" +
                    string.Join(",", dataNeeds.Select(u => _get_history_state_data_need_json(u))) + "]" +
                ",\"Connections\":[" +
                    string.Join(",", connections.Select(u => _get_history_connection_json(u))) + "]" +
                "}";
        }

        protected void create_history_form_instance(Guid historyId, Guid outStateId, Guid formId, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!_has_workflow_permission(WFController.get_history_owner_id(paramsContainer.Tenant.Id, historyId)))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                return;
            }

            Guid instanceId = WFController.create_history_form_instance(paramsContainer.Tenant.Id, historyId,
                outStateId, formId, paramsContainer.CurrentUserID.Value);

            responseText = instanceId == Guid.Empty ? "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}" :
                "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\",\"InstanceID\":\"" + instanceId.ToString() + "\"}";
        }

        protected void _send_message_to_audience(Guid workflowId, Guid inStateId, Guid outStateId, Guid? directorId,
            ref Dictionary<string, string> dic, Guid ownerCreatorUserId, string bodyText = "-1")
        {
            try
            {
                List<AutoMessage> automessages =
                    WFController.get_connection_auto_messages(paramsContainer.Tenant.Id, workflowId, inStateId, outStateId);

                if (automessages == null || automessages.Count == 0) return;

                foreach (AutoMessage aud in automessages)
                {
                    try
                    {
                        if (!aud.AudienceType.HasValue) continue;

                        string text = Expressions.replace(bodyText == "-1" ? aud.BodyText : bodyText, ref dic, Expressions.Patterns.AutoTag);

                        if (aud.AudienceType.Value == AudienceTypes.SendToOwner)
                        {
                            List<Guid> userIds = !directorId.HasValue ? new List<Guid>() :
                                CNController.get_node_creators(paramsContainer.Tenant.Id, directorId.Value)
                                .Select(u => u.User.UserID.Value).ToList();
                            if (!userIds.Any(u => u == ownerCreatorUserId)) userIds.Add(ownerCreatorUserId);

                            User su = UserUtilities.SystemUser(paramsContainer.Tenant.Id);

                            if (su != null && su.UserID.HasValue)
                            {
                                MSGController.bulk_send_message(paramsContainer.Tenant.Id,
                                    su.UserID.Value, userIds, "جریان کاری", text);
                            }
                        }
                        else if (aud.AudienceType.Value == AudienceTypes.SpecificNode)
                        {
                            if (!aud.Node.NodeID.HasValue) continue;
                            bool? admin = null;
                            if (aud.Admin.HasValue && aud.Admin.Value) admin = true;
                            List<Guid> userIds = CNController.get_member_user_ids(paramsContainer.Tenant.Id,
                                aud.Node.NodeID.Value, NodeMemberStatuses.Accepted, admin);

                            User su = UserUtilities.SystemUser(paramsContainer.Tenant.Id);

                            if (su != null && su.UserID.HasValue)
                            {
                                MSGController.bulk_send_message(paramsContainer.Tenant.Id,
                                    su.UserID.Value, userIds, "جریان کاری", text);
                            }
                        }
                        else if (aud.AudienceType.Value == AudienceTypes.RefState)
                        {
                            if (!aud.RefState.StateID.HasValue || !directorId.HasValue) continue;
                            Guid? lastOutStateId = WFController.get_last_selected_state_id(paramsContainer.Tenant.Id,
                                directorId.Value, aud.RefState.StateID.Value);
                            if (!lastOutStateId.HasValue || lastOutStateId.Value == Guid.Empty) continue;

                            _send_message_to_audience(workflowId, aud.RefState.StateID.Value, lastOutStateId.Value, directorId.Value,
                                ref dic, ownerCreatorUserId, aud.BodyText);
                        }
                    }
                    catch (Exception ex) { string strEx = ex.ToString(); }
                }
            }
            catch { }
        }

        public void find_director(Guid ownerId, Guid workflowId, Guid stateId,
            Node nodeObject, ref Guid? directorNodeId, ref Guid? directorUserId)
        {
            State st = WFController.get_workflow_state(paramsContainer.Tenant.Id, workflowId, stateId);

            if (st == null) return;

            switch (st.ResponseType)
            {
                case StateResponseTypes.SendToOwner:
                    {
                        if (nodeObject == null) nodeObject = CNController.get_node(paramsContainer.Tenant.Id, ownerId);
                        directorUserId = nodeObject?.Creator?.UserID;
                    }
                    break;
                case StateResponseTypes.ContentAdmin:
                    {
                        if (nodeObject == null) nodeObject = CNController.get_node(paramsContainer.Tenant.Id, ownerId);
                        List<Guid> contributors = CNController.get_node_creators(paramsContainer.Tenant.Id,
                            ownerId).Select(u => u.User.UserID.Value).ToList();
                        List<HierarchyAdmin> admins =
                            CNController.get_node_admins(paramsContainer.Tenant.Id, ownerId, nodeObject);

                        directorUserId = (new KnowledgeAPI() { paramsContainer = this.paramsContainer })
                            .get_main_admin(nodeObject, contributors, admins);
                    }
                    break;
                case StateResponseTypes.SpecificNode:
                    directorNodeId = st.DirectorNode.NodeID;
                    break;
                case StateResponseTypes.RefState:
                    {
                        History h = WFController.get_last_history(paramsContainer.Tenant.Id, ownerId, st.RefStateID, false);
                        if (h != null)
                        {
                            directorNodeId = h.DirectorNode?.NodeID;
                            directorUserId = h.DirectorUserID;
                        }
                    }
                    break;
                case StateResponseTypes.SpecificUser:
                    directorUserId = st.DirectorUser?.UserID;
                    break;
            }
        }

        protected void send_to_next_state(Guid historyId, Guid stateId, Guid? directorId, string description,
            ref List<DocFileInfo> attachedFiles, bool reject, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!string.IsNullOrEmpty(description) && description.Length > 1900)
            {
                responseText = "{\"ErrorText\":\"" + Messages.MaxAllowedInputLengthExceeded + "\"}";
                return;
            }

            if (directorId == Guid.Empty) directorId = null;

            Guid ownerId = WFController.get_history_owner_id(paramsContainer.Tenant.Id, historyId);

            if (!_has_workflow_permission(ownerId))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                return;
            }

            History history = new History()
            {
                HistoryID = historyId,
                Description = description,
                AttachedFiles = attachedFiles,
                SendDate = DateTime.Now
            };

            history.State.StateID = stateId;
            history.Sender.UserID = paramsContainer.CurrentUserID;

            bool directorIsNode = false;
            bool directorIsUser = false;

            if (directorId.HasValue)
            {
                directorIsNode = CNController.is_node(paramsContainer.Tenant.Id, directorId.Value);
                directorIsUser = !directorIsNode &&
                    UsersController.get_user(paramsContainer.Tenant.Id, directorId.Value) != null;

                if (directorIsNode) history.DirectorNode.NodeID = directorId;
                else if (directorIsUser)
                {
                    //Director will be determined later
                }
            }

            //find director
            History hist = WFController.get_history(paramsContainer.Tenant.Id, historyId);

            Guid? directorNodeId = null, directorUserId = null;

            if (hist != null && hist.WorkFlowID.HasValue) find_director(ownerId,
                hist.WorkFlowID.Value, stateId, null, ref directorNodeId, ref directorUserId);

            if (directorNodeId.HasValue) history.DirectorNode.NodeID = directorNodeId;
            if (directorUserId.HasValue) history.DirectorUserID = directorUserId;

            if (!reject && !history.DirectorNode.NodeID.HasValue && !history.DirectorUserID.HasValue)
            {
                responseText = "{\"ErrorText\":\"" + Messages.NoDirectorIsSet + "\"}";
                return;
            }
            //end of find director

            if(attachedFiles != null)
                attachedFiles.ForEach(f => f.move(FolderNames.TemporaryFiles, FolderNames.Attachments));

            string msg = string.Empty;
            List<Dashboard> sentDashboards = new List<Dashboard>();
            bool result = WFController.send_to_next_state(paramsContainer.Tenant.Id,
                history, reject, ref msg, ref sentDashboards);

            if (!result && attachedFiles != null)
                attachedFiles.ForEach(f => f.move(FolderNames.Attachments, FolderNames.TemporaryFiles));

            History oldHistory = new History();
            if (result)
            {
                oldHistory = WFController.get_history(paramsContainer.Tenant.Id, historyId);
                if (oldHistory != null) get_owner_history(oldHistory.OwnerID.Value, true, true, ref responseText);
                if (string.IsNullOrEmpty(responseText)) responseText = "{}";
            }

            responseText = !result ? "{\"ErrorText\":\"" + msg + "\"}" :
                "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\",\"History\":" + responseText + "}";

            if (result && oldHistory != null)
            {
                try
                {
                    Node node = new Node();
                    Dictionary<string, string> dic =
                        _get_replacement_dictionary(oldHistory.OwnerID.Value, description,
                        paramsContainer.CurrentUserID.Value, ref node);

                    _send_message_to_audience(oldHistory.WorkFlowID.Value, oldHistory.State.StateID.Value,
                        stateId, directorId, ref dic, node.Creator.UserID.Value);
                }
                catch (Exception ex) { string strEx = ex.ToString(); }
            }

            //Send Notification
            if (result && sentDashboards.Count > 0)
                NotificationController.transfer_dashboards(paramsContainer.Tenant.Id, sentDashboards);
            //end of Send Notification
        }

        protected void terminate_workflow(Guid historyId, string description, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!string.IsNullOrEmpty(description) && description.Length > 1900)
            {
                responseText = "{\"ErrorText\":\"" + Messages.MaxAllowedInputLengthExceeded + "\"}";
                return;
            }

            if (!_has_workflow_permission(WFController.get_history_owner_id(paramsContainer.Tenant.Id, historyId)))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                _save_error_log(Modules.Log.Action.TerminateWorkFlow_PermissionFailed, historyId);
                return;
            }

            string errorText = string.Empty;
            bool result = WFController.terminate_workFlow(paramsContainer.Tenant.Id, historyId,
                description, paramsContainer.CurrentUserID.Value, ref errorText);

            responseText = result ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + (string.IsNullOrEmpty(errorText) ? Messages.OperationFailed.ToString() : errorText) + "\"}";

            //Save Log
            if (result)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = DateTime.Now,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.TerminateWorkFlow,
                    SubjectID = historyId,
                    ModuleIdentifier = ModuleIdentifier.WF
                });
            }
            //end of Save Log
        }

        protected void restart_workflow(Guid ownerId, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            if (!_has_workflow_permission(ownerId))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                _save_error_log(Modules.Log.Action.RestartWorkFlow_PermissionFailed, ownerId);
                return;
            }

            List<Dashboard> sentDashboards = new List<Dashboard>();
            string errorText = string.Empty;

            //Determine director
            Guid? directorNodeId = null, directorUserId = null;

            History hist = WFController.get_last_history(paramsContainer.Tenant.Id, ownerId);
            State firstState = hist == null || !hist.WorkFlowID.HasValue ? null :
                WFController.get_first_workflow_state(paramsContainer.Tenant.Id, hist.WorkFlowID.Value);
            Node node = CNController.get_node(paramsContainer.Tenant.Id, ownerId);

            if (firstState != null && firstState.StateID.HasValue && node != null)
            {
                find_director(node.NodeID.Value, hist.WorkFlowID.Value, firstState.StateID.Value, node,
                    ref directorNodeId, ref directorUserId);
            }
            //end of Determine director

            bool result = WFController.restart_workFlow(paramsContainer.Tenant.Id, ownerId, directorNodeId, directorUserId,
                paramsContainer.CurrentUserID.Value, ref sentDashboards, ref errorText);

            responseText = result ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + (string.IsNullOrEmpty(errorText) ? Messages.OperationFailed.ToString() : errorText) + "\"}";

            //Send Notification
            if (result && sentDashboards.Count > 0)
                NotificationController.transfer_dashboards(paramsContainer.Tenant.Id, sentDashboards);
            //end of Send Notification

            //Save Log
            if (result)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = DateTime.Now,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.RestartWorkFlow,
                    SubjectID = ownerId,
                    ModuleIdentifier = ModuleIdentifier.WF
                });
            }
            //end of Save Log
        }

        protected void get_workflow_owners(ref string responseText)
        {
            if (!paramsContainer.GBEdit) return;

            List<NodeType> nodeTypes = WFController.get_workflow_owners(paramsContainer.Tenant.Id);

            responseText = "{\"NodeTypes\":[";

            bool isFirst = true;
            foreach (NodeType nt in nodeTypes)
            {
                responseText += (isFirst ? string.Empty : ",") +
                    "{\"NodeTypeID\":\"" + nt.NodeTypeID.ToString() + "\",\"Name\":\"" + Base64.encode(nt.Name) + "\"}";
                isFirst = false;
            }

            responseText += "]}";
        }

        public void get_service_abstract(Guid nodeTypeId, Guid? workflowId, Guid? userId,
            string nullTagLabel, bool serviceUsersCount, ref string responseText)
        {
            //if (!paramsContainer.GBEdit) return;

            if (workflowId == Guid.Empty) workflowId = null;
            if (userId == Guid.Empty) userId = null;

            NodeType nodeType = CNController.get_node_type(paramsContainer.Tenant.Id, nodeTypeId);
            List<KeyValuePair<string, int>> items =
                WFController.get_service_abstract(paramsContainer.Tenant.Id, nodeTypeId, workflowId, userId, nullTagLabel);

            string strUsersCount = string.Empty;
            if (serviceUsersCount)
            {
                int usersCount = serviceUsersCount ?
                    WFController.get_service_user_ids(paramsContainer.Tenant.Id, nodeTypeId, workflowId).Count : 0;
                strUsersCount = "\",\"UsersCount\":\"" + usersCount.ToString();
            }

            string nodeTypeName = nodeType == null ? string.Empty : nodeType.Name;
            Base64.encode(nodeTypeName, ref nodeTypeName);

            responseText = "{\"WorkFlowID\":\"" + (workflowId.HasValue ? workflowId.ToString() : string.Empty) +
                "\",\"NodeType\":\"" + nodeTypeName + strUsersCount + "\",\"Items\":[";

            bool isFirst = true;
            foreach (KeyValuePair<string, int> _item in items)
            {
                string tag = _item.Key;
                int count = _item.Value;

                Base64.encode(tag, ref tag);

                responseText += (isFirst ? string.Empty : ",") + "{\"Tag\":\"" + tag + "\",\"Count\":\"" + count.ToString() + "\"}";
                isFirst = false;
            } //end of 'foreach (State st in states)'

            responseText += "]}";
        }

        protected void add_owner_workflow(Guid nodeTypeId, Guid workflowId, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            List<AccessRoleName> art = AuthorizationManager.has_right(new List<AccessRoleName>() {
                AccessRoleName.ManageWorkflow,
                AccessRoleName.ManageOntology
            }, paramsContainer.CurrentUserID);

            if (!art.Any(u => u == AccessRoleName.ManageOntology) && !art.Any(u => u == AccessRoleName.ManageWorkflow))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                _save_error_log(Modules.Log.Action.AddOwnerWorkFlow_PermissionFailed, nodeTypeId, workflowId);
                return;
            }

            bool result = WFController.add_owner_workflow(paramsContainer.Tenant.Id,
                nodeTypeId, workflowId, paramsContainer.CurrentUserID.Value);

            responseText = result ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";

            //Save Log
            if (result)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = DateTime.Now,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.AddOwnerWorkFlow,
                    SubjectID = nodeTypeId,
                    SecondSubjectID = workflowId,
                    ModuleIdentifier = ModuleIdentifier.WF
                });
            }
            //end of Save Log
        }

        protected void remove_owner_workflow(Guid nodeTypeId, Guid workflowId, ref string responseText)
        {
            //Privacy Check: OK
            if (!paramsContainer.GBEdit) return;

            List<AccessRoleName> art = AuthorizationManager.has_right(new List<AccessRoleName>() {
                AccessRoleName.ManageWorkflow,
                AccessRoleName.ManageOntology
            }, paramsContainer.CurrentUserID);

            if (!art.Any(u => u == AccessRoleName.ManageOntology) && !art.Any(u => u == AccessRoleName.ManageWorkflow))
            {
                responseText = "{\"ErrorText\":\"" + Messages.AccessDenied + "\"}";
                _save_error_log(Modules.Log.Action.RemoveOwnerWorkFlow_PermissionFailed, nodeTypeId, workflowId);
                return;
            }

            bool result = WFController.remove_owner_workflow(paramsContainer.Tenant.Id,
                nodeTypeId, workflowId, paramsContainer.CurrentUserID.Value);

            responseText = result ? "{\"Succeed\":\"" + Messages.OperationCompletedSuccessfully + "\"}" :
                "{\"ErrorText\":\"" + Messages.OperationFailed + "\"}";

            //Save Log
            if (result)
            {
                LogController.save_log(paramsContainer.Tenant.Id, new Log()
                {
                    UserID = paramsContainer.CurrentUserID,
                    Date = DateTime.Now,
                    HostAddress = PublicMethods.get_client_ip(HttpContext.Current),
                    HostName = PublicMethods.get_client_host_name(HttpContext.Current),
                    Action = Modules.Log.Action.RemoveOwnerWorkFlow,
                    SubjectID = nodeTypeId,
                    SecondSubjectID = workflowId,
                    ModuleIdentifier = ModuleIdentifier.WF
                });
            }
            //end of Save Log
        }

        protected void get_owner_workflows(Guid nodeTypeId, ref string responseText)
        {
            if (!paramsContainer.GBEdit) return;

            Dictionary<Guid, string> dic = new Dictionary<Guid, string>();

            responseText = "[" + ProviderUtil.list_to_string<string>(
                WFController.get_owner_workflows(paramsContainer.Tenant.Id, nodeTypeId)
                .Select(u => _get_workflow_json(u, ref dic)).ToList()) + "]";
        }

        protected void get_owner_workflow_primary_key(Guid nodeTypeId, Guid workflowId, ref string responseText)
        {
            if (!paramsContainer.GBEdit) return;

            Guid? pk = WFController.get_owner_workflow_primary_key(paramsContainer.Tenant.Id, nodeTypeId, workflowId);
            responseText = pk.HasValue ? "\"" + pk.ToString() + "\"" : "null";
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