using System;
namespace Spine.Common.Attributes
{
    public class SortableAttribute : Attribute
    {
        public string Columns { get; set; }
        public bool IsDefault { get; set; }

        /// <summary>
        /// there should be only 1 unique property per model
        /// </summary>
        public bool IsUnique { get; set; }

        public SortableAttribute(string columns)
        {
            Columns = columns;
        }
    }

}
