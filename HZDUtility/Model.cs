using Decima;

namespace HZDUtility
{
    public abstract class Model
    {
        public string Name { get; set; }
        public BaseGGUUID Id { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}