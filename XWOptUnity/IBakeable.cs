namespace SchmooTech.XWOptUnity
{
    internal interface IBakeable
    {
        void ParallelizableBake(int? degreesOfParallelism);
        void MainThreadBake();
    }
}
