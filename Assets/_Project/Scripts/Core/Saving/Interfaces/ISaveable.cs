namespace Zone8.Saving.Interfaces
{

    public interface ISaveable
    {

        object CaptureState();

        void RestoreState(object state);
    }
}