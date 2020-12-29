using System.Threading.Tasks;
using Decima;

namespace HZDUtility.Models
{
    public class Outfit : Model
    {
        public bool Modified { get; set; }
        public BaseGGUUID RefId { get; set; }
        public BaseGGUUID LocalNameId { get; set; }
        public string LocalNameFile { get; set; }
        public string DisplayName { get; set; }

        public Outfit Clone()
        {
            return new Outfit()
            {
                Id = BaseGGUUID.FromOther(Id),
                Name = Name,
                RefId = BaseGGUUID.FromOther(RefId),
                LocalNameId = BaseGGUUID.FromOther(LocalNameId),
                LocalNameFile = LocalNameFile
            };
        }

        public override string ToString()
        {
            var modified = Modified ? " *" : "";
            return Name + modified;
        }
    }
}
