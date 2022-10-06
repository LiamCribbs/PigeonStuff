namespace Pigeon.Sequencer
{
    /// <summary>
    /// Reference wrapper for an int
    /// </summary>
    [System.Serializable]
    public class RefInt
    {
        public int value = -1;

        public RefInt()
        {

        }

        public RefInt(int value)
        {
            this.value = value;
        }
    }
}