namespace AutoRegister
{
    public abstract class PrimitiveWrapperBase<T> where T : notnull, IComparable<T>
    {
        protected readonly T id;

        public PrimitiveWrapperBase(T id)
        {
            this.id = id;
        }

        public override string ToString() => id.ToString() ?? "";
        public override bool Equals(object? obj) => obj is PrimitiveWrapperBase<T> typed ? typed.id.Equals(id) : false;
        public override int GetHashCode() => ToString().GetHashCode();
    }
}
