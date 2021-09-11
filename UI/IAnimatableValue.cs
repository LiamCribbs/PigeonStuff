namespace Pigeon
{
    public interface IAnimatableValue<T>
    {
        void SetValue(T value);
        T GetValue();
    }
}