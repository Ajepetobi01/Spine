using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Core.ManageSubcription.ViewModel
{
    public class SubscriberNoteViewModel
    {
        public int ID_Note { get; set; }
        public Guid CompanyId { get; set; }
        public string Description { get; set; }
        public string DateCreated { get; set; }
    }
    public class NoteRequest
    {
        public int noteId { get; set; }
        public Guid CompanyId { get; set; }
        public string Description { get; set; }
    }
}
