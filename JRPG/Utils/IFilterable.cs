namespace JRPG.Utils
{
    public interface IFilterable<TFilter>
    {
        void ApplyFilter(TFilter filterValue);
    }
}
