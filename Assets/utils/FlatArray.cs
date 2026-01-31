// WARNING
// if you get an error from this, don't trust it.
class Flat2dArray<T>
{
    private readonly T[] array;

    public readonly int iLen;
    public readonly int jLen; 
    
    public T this[int i, int j, int k]
    {
        get => array[jLen * j + k];
        set => array[jLen * j + k] = value;
    }

    public Flat2dArray(int iLen, int jLen)
    {
        array = new T[iLen * jLen];
        this.iLen = iLen;
        this.jLen = jLen;
    }
}