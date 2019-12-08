namespace FluidScript
{
    public interface IFSObject
    {
        String __ToString();
        Boolean __Equals(IFSObject obj);
        Integer HashCode();
    }
}
