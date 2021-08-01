using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NpgsqlTypes;

namespace RaaiVan.Modules.GlobalUtilities.DBCompositeTypes
{
    public class FormElementTableType : ITableType
    {
        public FormElementTableType() : base() { } //empty constructor is a must

        public string MSSQLName { get { return "FormElementTableType"; } }

        [PgName("element_id")]
        public Guid? ElementID;

        [PgName("template_element_id")]
        public Guid? TemplateElementID;

        [PgName("instance_id")]
        public Guid? InstanceID;

        [PgName("ref_element_id")]
        public Guid? RefElementID;

        [PgName("title")]
        public string Title;

        [PgName("name")]
        public string Name;

        [PgName("sequence_number")]
        public int? SequenceNumber;

        [PgName("necessary")]
        public bool? Necessary;

        [PgName("unique_value")]
        public bool? UniqueValue;

        [PgName("type")]
        public string Type;

        [PgName("help")]
        public string Help;

        [PgName("info")]
        public string Info;

        [PgName("weight")]
        public double? Weight;

        [PgName("text_value")]
        public string TextValue;

        [PgName("float_value")]
        public double? FloatValue;

        [PgName("bit_value")]
        public bool? BitValue;

        [PgName("date_value")]
        public DateTime? DateValue;


        public FormElementTableType(Guid? elementId, Guid? templateElementId, Guid? instanceId, Guid? refElementId,
            string title, string name, int? sequenceNumber, bool? necessary, bool? uniqueValue, string type, string help,
            string info, double? weight, string textValue, double? floatValue, bool? bitValue, DateTime? dateValue)
        {
            ElementID = elementId;
            TemplateElementID = templateElementId;
            InstanceID = instanceId;
            RefElementID = refElementId;
            Title = title;
            Name = name;
            SequenceNumber = sequenceNumber;
            Necessary = necessary;
            UniqueValue = uniqueValue;
            Type = type;
            Help = help;
            Info = info;
            Weight = weight;
            TextValue = textValue;
            FloatValue = floatValue;
            BitValue = bitValue;
            DateValue = dateValue;
        }

        public object[] to_array()
        {
            return new List<object>() {
                ElementID,
                TemplateElementID,
                InstanceID,
                RefElementID,
                Title,
                Name,
                SequenceNumber,
                Necessary,
                UniqueValue,
                Type,
                Help,
                Info,
                Weight,
                TextValue,
                FloatValue,
                BitValue,
                DateValue
            }.ToArray();
        }

        public FormElementTableType[] get_array(List<FormElementTableType> list)
        {
            return list.ToArray();
        }
    }
}
