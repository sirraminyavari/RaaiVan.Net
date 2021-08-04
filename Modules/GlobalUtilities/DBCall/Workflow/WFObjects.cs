using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RaaiVan.Modules.Users;
using RaaiVan.Modules.CoreNetwork;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.FormGenerator;

namespace RaaiVan.Modules.WorkFlow
{
    public enum StateResponseTypes
    {
        None,
        SendToOwner,
        RefState,
        SpecificNode,
        ContentAdmin
    }

    public enum StateDataNeedsTypes
    {
        None,
        RefState,
        SpecificNodes
    }

    public enum AudienceTypes
    {
        SendToOwner,
        RefState,
        SpecificNode
    }

    public enum ViewerStatus
    {
        None,
        NotInWorkFlow,
        Director,
        DirectorNodeMember,
        Owner
    }

    public class StateDataNeedInstance
    {
        private Guid? _InstanceID;
        private Guid? _HistoryID;
        private Guid? _OwnerID;
        private Node _DirectorNode;
        private bool? _Admin;
        private bool? _Filled;
        private DateTime? _FillingDate;
        private Guid? _FormID;
        private Guid? _AttachmentID;
        private Guid? _CreatorUserID;
        private DateTime? _CreationDate;
        private Guid? _LastModifierUserID;
        private DateTime? _LastModificationDate;
        private List<DocFileInfo> _PreAttachedFiles;
        private List<DocFileInfo> _Attachments;


        public StateDataNeedInstance()
        {
            _DirectorNode = new Node();
            _PreAttachedFiles = new List<DocFileInfo>();
            _Attachments = new List<DocFileInfo>();
        }

        public Guid? InstanceID
        {
            get { return _InstanceID; }
            set { _InstanceID = value; }
        }

        public Guid? HistoryID
        {
            get { return _HistoryID; }
            set { _HistoryID = value; }
        }

        public Guid? OwnerID
        {
            get { return _OwnerID; }
            set { _OwnerID = value; }
        }

        public Node DirectorNode
        {
            get { return _DirectorNode; }
            set { _DirectorNode = value; }
        }

        public bool? Admin
        {
            get { return _Admin; }
            set { _Admin = value; }
        }

        public Guid? FormID
        {
            get { return _FormID; }
            set { _FormID = value; }
        }

        public bool? Filled
        {
            get { return _Filled; }
            set { _Filled = value; }
        }

        public DateTime? FillingDate
        {
            get { return _FillingDate; }
            set { _FillingDate = value; }
        }

        public Guid? AttachmentID
        {
            get { return _AttachmentID; }
            set { _AttachmentID = value; }
        }

        public Guid? CreatorUserID
        {
            get { return _CreatorUserID; }
            set { _CreatorUserID = value; }
        }

        public DateTime? CreationDate
        {
            get { return _CreationDate; }
            set { _CreationDate = value; }
        }

        public Guid? LastModifierUserID
        {
            get { return _LastModifierUserID; }
            set { _LastModifierUserID = value; }
        }

        public DateTime? LastModificationDate
        {
            get { return _LastModificationDate; }
            set { _LastModificationDate = value; }
        }

        public List<DocFileInfo> PreAttachedFiles
        {
            get { return _PreAttachedFiles; }
            set { _PreAttachedFiles = value; }
        }

        public List<DocFileInfo> Attachments
        {
            get { return _Attachments; }
            set { _Attachments = value; }
        }
    }

    public class StateDataNeed
    {
        private Guid? _ID;
        private Guid? _WorkFlowID;
        private Guid? _StateID;
        private NodeType _DirectorNodeType;
        private Guid? _FormID;
        private string _FormTitle;
        private string _Description;
        private bool? _MultiSelect;
        private bool? _Admin;
        private bool? _Necessary;
        private Guid? _CreatorUserID;
        private DateTime? _CreationDate;
        private Guid? _LastModifierUserID;
        private DateTime? _LastModificationDate;
        private List<StateDataNeedInstance> _Instances;


        public StateDataNeed()
        {
            _DirectorNodeType = new NodeType();
            _Instances = new List<StateDataNeedInstance>();
        }

        public Guid? ID
        {
            get { return _ID; }
            set { _ID = value; }
        }

        public Guid? WorkFlowID
        {
            get { return _WorkFlowID; }
            set { _WorkFlowID = value; }
        }

        public Guid? StateID
        {
            get { return _StateID; }
            set { _StateID = value; }
        }

        public NodeType DirectorNodeType
        {
            get { return _DirectorNodeType; }
            set { _DirectorNodeType = value; }
        }

        public Guid? FormID
        {
            get { return _FormID; }
            set { _FormID = value; }
        }

        public string FormTitle
        {
            get { return _FormTitle; }
            set { _FormTitle = value; }
        }

        public string Description
        {
            get { return _Description; }
            set { _Description = value; }
        }

        public bool? MultiSelect
        {
            get { return _MultiSelect; }
            set { _MultiSelect = value; }
        }

        public bool? Admin
        {
            get { return _Admin; }
            set { _Admin = value; }
        }

        public bool? Necessary
        {
            get { return _Necessary; }
            set { _Necessary = value; }
        }

        public Guid? CreatorUserID
        {
            get { return _CreatorUserID; }
            set { _CreatorUserID = value; }
        }

        public DateTime? CreationDate
        {
            get { return _CreationDate; }
            set { _CreationDate = value; }
        }

        public Guid? LastModifierUserID
        {
            get { return _LastModifierUserID; }
            set { _LastModifierUserID = value; }
        }

        public DateTime? LastModificationDate
        {
            get { return _LastModificationDate; }
            set { _LastModificationDate = value; }
        }

        public List<StateDataNeedInstance> Instances
        {
            get { return _Instances; }
            set { _Instances = value; }
        }
    }

    public class State
    {
        private Guid? _ID;
        private Guid? _StateID;
        private Guid? _WorkFlowID;
        private string _Title;
        private string _Description;
        private string _Tag;
        private StateResponseTypes? _ResponseType;
        private Guid? _RefStateID;
        private Node _DirectorNode;
        private bool? _DirectorIsAdmin;
        private StateDataNeedsTypes? _DataNeedsType;
        private Guid? _RefDataNeedsStateID;
        private string _DataNeedsDescription;
        private bool? _DescriptionNeeded;
        private bool? _HideOwnerName;
        private bool? _EditPermission;
        private bool? _FreeDataNeedRequests;
        private List<StateDataNeed> _DataNeeds;
        private int? _MaxAllowedRejections;
        private string _RejectionTitle;
        private Guid? _RejectionRefStateID;
        private string _RejectionRefStateTitle;
        private Guid? _PollID;
        private string _PollName;
        private List<StateConnection> _Connections;
        private Guid? _CreatorUserID;
        private DateTime? _CreationDate;
        private Guid? _LastModifierUserID;
        private DateTime? _LastModificationDate;


        public State()
        {
            _DirectorNode = new Node();
            _DataNeeds = new List<StateDataNeed>();
        }

        public Guid? ID
        {
            get { return _ID; }
            set { _ID = value; }
        }

        public Guid? StateID
        {
            get { return _StateID; }
            set { _StateID = value; }
        }

        public string Title
        {
            get { return _Title; }
            set { _Title = value; }
        }

        public string Description
        {
            get { return _Description; }
            set { _Description = value; }
        }

        public string Tag
        {
            get { return _Tag; }
            set { _Tag = value; }
        }

        public StateResponseTypes? ResponseType
        {
            get { return _ResponseType; }
            set { _ResponseType = value; }
        }

        public Guid? WorkFlowID
        {
            get { return _WorkFlowID; }
            set { _WorkFlowID = value; }
        }

        public Guid? RefStateID
        {
            get { return _RefStateID; }
            set { _RefStateID = value; }
        }

        public Node DirectorNode
        {
            get { return _DirectorNode; }
            set { _DirectorNode = value; }
        }

        public bool? DirectorIsAdmin
        {
            get { return _DirectorIsAdmin; }
            set { _DirectorIsAdmin = value; }
        }

        public StateDataNeedsTypes? DataNeedsType
        {
            get { return _DataNeedsType; }
            set { _DataNeedsType = value; }
        }

        public Guid? RefDataNeedsStateID
        {
            get { return _RefDataNeedsStateID; }
            set { _RefDataNeedsStateID = value; }
        }

        public string DataNeedsDescription
        {
            get { return _DataNeedsDescription; }
            set { _DataNeedsDescription = value; }
        }

        public bool? DescriptionNeeded
        {
            get { return _DescriptionNeeded; }
            set { _DescriptionNeeded = value; }
        }

        public bool? HideOwnerName
        {
            get { return _HideOwnerName; }
            set { _HideOwnerName = value; }
        }

        public bool? EditPermission
        {
            get { return _EditPermission; }
            set { _EditPermission = value; }
        }

        public bool? FreeDataNeedRequests
        {
            get { return _FreeDataNeedRequests; }
            set { _FreeDataNeedRequests = value; }
        }

        public List<StateDataNeed> DataNeeds
        {
            get { return _DataNeeds; }
            set { _DataNeeds = value; }
        }

        public int? MaxAllowedRejections
        {
            get { return _MaxAllowedRejections; }
            set { _MaxAllowedRejections = value; }
        }

        public string RejectionTitle
        {
            get { return _RejectionTitle; }
            set { _RejectionTitle = value; }
        }

        public Guid? RejectionRefStateID
        {
            get { return _RejectionRefStateID; }
            set { _RejectionRefStateID = value; }
        }

        public string RejectionRefStateTitle
        {
            get { return _RejectionRefStateTitle; }
            set { _RejectionRefStateTitle = value; }
        }

        public Guid? PollID
        {
            get { return _PollID; }
            set { _PollID = value; }
        }

        public string PollName
        {
            get { return _PollName; }
            set { _PollName = value; }
        }

        public List<StateConnection> Connections
        {
            get { return _Connections; }
            set { _Connections = value; }
        }

        public Guid? CreatorUserID
        {
            get { return _CreatorUserID; }
            set { _CreatorUserID = value; }
        }

        public DateTime? CreationDate
        {
            get { return _CreationDate; }
            set { _CreationDate = value; }
        }

        public Guid? LastModifierUserID
        {
            get { return _LastModifierUserID; }
            set { _LastModifierUserID = value; }
        }

        public DateTime? LastModificationDate
        {
            get { return _LastModificationDate; }
            set { _LastModificationDate = value; }
        }
    }

    public class StateConnectionForm
    {
        private Guid? _WorkFlowID;
        private Guid? _InStateID;
        private Guid? _OutStateID;
        private FormType _Form;
        private string _Description;
        private bool? _Necessary;
        private Guid? _CreatorUserID;
        private DateTime? _CreationDate;
        private Guid? _LastModifierUserID;
        private DateTime? _LastModificationDate;


        public StateConnectionForm()
        {
            _Form = new FormType();
        }

        public Guid? WorkFlowID
        {
            get { return _WorkFlowID; }
            set { _WorkFlowID = value; }
        }

        public Guid? InStateID
        {
            get { return _InStateID; }
            set { _InStateID = value; }
        }

        public Guid? OutStateID
        {
            get { return _OutStateID; }
            set { _OutStateID = value; }
        }

        public FormType Form
        {
            get { return _Form; }
            set { _Form = value; }
        }

        public string Description
        {
            get { return _Description; }
            set { _Description = value; }
        }

        public bool? Necessary
        {
            get { return _Necessary; }
            set { _Necessary = value; }
        }

        public Guid? CreatorUserID
        {
            get { return _CreatorUserID; }
            set { _CreatorUserID = value; }
        }

        public DateTime? CreationDate
        {
            get { return _CreationDate; }
            set { _CreationDate = value; }
        }

        public Guid? LastModifierUserID
        {
            get { return _LastModifierUserID; }
            set { _LastModifierUserID = value; }
        }

        public DateTime? LastModificationDate
        {
            get { return _LastModificationDate; }
            set { _LastModificationDate = value; }
        }
    }

    public class AutoMessage
    {
        private Guid? _AutoMessageID;
        private Guid? _OwnerID;
        private Guid? _WorkFlowID;
        private Guid? _InStateID;
        private Guid? _OutStateID;
        private string _BodyText;
        private AudienceTypes? _AudienceType;
        private State _RefState;
        private Node _Node;
        private bool? _Admin;
        private Guid? _CreatorUserID;
        private DateTime? _CreationDate;
        private Guid? _LastModifierUserID;
        private DateTime? _LastModificationDate;


        public AutoMessage()
        {
            _RefState = new State();
            _Node = new Node();
        }

        public Guid? AutoMessageID
        {
            get { return _AutoMessageID; }
            set { _AutoMessageID = value; }
        }

        public Guid? OwnerID
        {
            get { return _OwnerID; }
            set { _OwnerID = value; }
        }

        public Guid? WorkFlowID
        {
            get { return _WorkFlowID; }
            set { _WorkFlowID = value; }
        }

        public Guid? InStateID
        {
            get { return _InStateID; }
            set { _InStateID = value; }
        }

        public Guid? OutStateID
        {
            get { return _OutStateID; }
            set { _OutStateID = value; }
        }

        public string BodyText
        {
            get { return _BodyText; }
            set { _BodyText = value; }
        }

        public AudienceTypes? AudienceType
        {
            get { return _AudienceType; }
            set { _AudienceType = value; }
        }

        public State RefState
        {
            get { return _RefState; }
            set { _RefState = value; }
        }

        public Node Node
        {
            get { return _Node; }
            set { _Node = value; }
        }

        public bool? Admin
        {
            get { return _Admin; }
            set { _Admin = value; }
        }

        public Guid? CreatorUserID
        {
            get { return _CreatorUserID; }
            set { _CreatorUserID = value; }
        }

        public DateTime? CreationDate
        {
            get { return _CreationDate; }
            set { _CreationDate = value; }
        }

        public Guid? LastModifierUserID
        {
            get { return _LastModifierUserID; }
            set { _LastModifierUserID = value; }
        }

        public DateTime? LastModificationDate
        {
            get { return _LastModificationDate; }
            set { _LastModificationDate = value; }
        }
    }

    public class StateConnection
    {
        private Guid? _ID;
        private Guid? _WorkFlowID;
        private State _InState;
        private State _OutState;
        private int? _SequenceNumber;
        private string _Label;
        private bool? _AttachmentRequired;
        private string _AttachmentTitle;
        private bool? _NodeRequired;
        private NodeType _DirectorNodeType;
        private string _NodeTypeDescription;

        private string _StateTitle;
        private Guid? _NodeID;
        private string _NodeTitle;

        private Guid? _CreatorUserID;
        private DateTime? _CreationDate;
        private Guid? _LastModifierUserID;
        private DateTime? _LastModificationDate;
        private List<DocFileInfo> _AttachedFiles;
        private List<StateConnectionForm> _Forms;
        private List<AutoMessage> _AutoMessages;
        private List<HistoryFormInstance> _HistoryFormInstances;

        public StateConnection()
        {
            _InState = new State();
            _OutState = new State();
            _DirectorNodeType = new NodeType();
            _AttachedFiles = new List<DocFileInfo>();
            _Forms = new List<StateConnectionForm>();
            _AutoMessages = new List<AutoMessage>();
            _HistoryFormInstances = new List<HistoryFormInstance>();
        }

        public Guid? ID
        {
            get { return _ID; }
            set { _ID = value; }
        }

        public Guid? WorkFlowID
        {
            get { return _WorkFlowID; }
            set { _WorkFlowID = value; }
        }

        public string StateTitle
        {
            get { return _StateTitle; }
            set { _StateTitle = value; }
        }

        public Guid? NodeID
        {
            get { return _NodeID; }
            set { _NodeID = value; }
        }

        public string NodeTitle
        {
            get { return _NodeTitle; }
            set { _NodeTitle = value; }
        }

        public State InState
        {
            get { return _InState; }
            set { _InState = value; }
        }

        public State OutState
        {
            get { return _OutState; }
            set { _OutState = value; }
        }

        public int? SequenceNumber
        {
            get { return _SequenceNumber; }
            set { _SequenceNumber = value; }
        }

        public string Label
        {
            get { return _Label; }
            set { _Label = value; }
        }

        public bool? AttachmentRequired
        {
            get { return _AttachmentRequired; }
            set { _AttachmentRequired = value; }
        }

        public string AttachmentTitle
        {
            get { return _AttachmentTitle; }
            set { _AttachmentTitle = value; }
        }

        public bool? NodeRequired
        {
            get { return _NodeRequired; }
            set { _NodeRequired = value; }
        }

        public NodeType DirectorNodeType
        {
            get { return _DirectorNodeType; }
            set { _DirectorNodeType = value; }
        }

        public string NodeTypeDescription
        {
            get { return _NodeTypeDescription; }
            set { _NodeTypeDescription = value; }
        }

        public Guid? CreatorUserID
        {
            get { return _CreatorUserID; }
            set { _CreatorUserID = value; }
        }

        public DateTime? CreationDate
        {
            get { return _CreationDate; }
            set { _CreationDate = value; }
        }

        public Guid? LastModifierUserID
        {
            get { return _LastModifierUserID; }
            set { _LastModifierUserID = value; }
        }

        public DateTime? LastModificationDate
        {
            get { return _LastModificationDate; }
            set { _LastModificationDate = value; }
        }

        public List<DocFileInfo> AttachedFiles
        {
            get { return _AttachedFiles; }
            set { _AttachedFiles = value; }
        }

        public List<StateConnectionForm> Forms
        {
            get { return _Forms; }
            set { _Forms = value; }
        }

        public List<AutoMessage> AutoMessages
        {
            get { return _AutoMessages; }
            set { _AutoMessages = value; }
        }

        public List<HistoryFormInstance> HistoryFormInstances
        {
            get { return _HistoryFormInstances; }
            set { _HistoryFormInstances = value; }
        }
    }

    public class WorkFlow
    {
        public Guid? WorkFlowID;
        public string Name;
        public string Description;
        public List<State> States;
        public Guid? CreatorUserID;
        public DateTime? CreationDate;
        public Guid? LastModifierUserID;
        public DateTime? LastModificationDate;

        public WorkFlow()
        {
            States = new List<State>();
        }

        public string toJson() {
            return "{\"WorkFlowID\":\"" + WorkFlowID.ToString() + "\"" + 
                ",\"Name\":\"" + Base64.encode(Name) + "\"" + 
                ",\"Description\":\"" + Base64.encode(Description) + "\"" + 
                "}";
        }
    }

    public class HistoryFormInstance
    {
        private Guid? _HistoryID;
        private Guid? _OutStateID;
        private Guid? _FormsID;
        private List<FormType> _Forms;


        public HistoryFormInstance()
        {
            _Forms = new List<FormType>();
        }

        public Guid? HistoryID
        {
            get { return _HistoryID; }
            set { _HistoryID = value; }
        }

        public Guid? OutStateID
        {
            get { return _OutStateID; }
            set { _OutStateID = value; }
        }

        public Guid? FormsID
        {
            get { return _FormsID; }
            set { _FormsID = value; }
        }

        public List<FormType> Forms
        {
            get { return _Forms; }
            set { _Forms = value; }
        }
    }

    public class History
    {
        public Guid? HistoryID;
        public Guid? PreviousHistoryID;
        public Guid? OwnerID;
        public Node DirectorNode;
        public Guid? DirectorUserID;
        public Guid? WorkFlowID;
        public State State;
        public Guid? SelectedOutStateID;
        public string Description;
        public User Sender;
        public DateTime? SendDate;
        public Guid? PollID;
        public string PollName;
        public List<HistoryFormInstance> FormInstances;
        public List<DocFileInfo> AttachedFiles;


        public History()
        {
            DirectorNode = new Node();
            State = new State();
            FormInstances = new List<HistoryFormInstance>();
            AttachedFiles = new List<DocFileInfo>();
            Sender = new User();
        }
    }

    public class WFDashboard
    {
        private Guid? _NodeID;
        private string _NodeName;
        private string _NodeType;
        private Guid? _InstanceID;
        private DateTime? _CreationDate;
        private string _StateTitle;

        public Guid? NodeID
        {
            get { return _NodeID; }
            set { _NodeID = value; }
        }

        public string NodeName
        {
            get { return _NodeName; }
            set { _NodeName = value; }
        }

        public string NodeType
        {
            get { return _NodeType; }
            set { _NodeType = value; }
        }

        public Guid? InstanceID
        {
            get { return _InstanceID; }
            set { _InstanceID = value; }
        }

        public DateTime? CreationDate
        {
            get { return _CreationDate; }
            set { _CreationDate = value; }
        }

        public string StateTitle
        {
            get { return _StateTitle; }
            set { _StateTitle = value; }
        }
    }

    public class WorkFlowNode
    {
        private State _State;
        private Node _Node;

        public WorkFlowNode()
        {
            _State = new State();
            _Node = new Node();
        }

        public State State
        {
            get { return _State; }
            set { _State = value; }
        }

        public Node Node
        {
            get { return _Node; }
            set { _Node = value; }
        }
    }
}
