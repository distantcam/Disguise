namespace TiviT.NCloak.CloakTasks
{
    public interface ICloakTask
    {
        string Name { get; }

        void RunTask();
    }
}