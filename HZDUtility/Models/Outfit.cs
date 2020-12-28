namespace HZDUtility.Models
{
    public class Outfit : Model
    {
        public bool Modified { get; set; }

        public override string ToString()
        {
            var modified = Modified ? " *" : "";

            var key = "playercostume_";
            var idx = Name.LastIndexOf(key) + key.Length;

            if (idx >= 0)
                return Name.Substring(idx) + modified;

            return Name + modified;
        }
    }
}
