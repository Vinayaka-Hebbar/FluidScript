namespace FluidScript
{
    public interface IFSObject
    {
        String __ToString();
        Boolean Equals(IFSObject obj);
        Integer HashCode();
    }
}
