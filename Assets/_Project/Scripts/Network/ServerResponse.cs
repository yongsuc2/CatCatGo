namespace CatCatGo.Network
{
    public class ServerResponse<T>
    {
        public bool Success;
        public string Error;
        public string ErrorCode;
        public T Data;
        public StateDelta Delta;
    }
}
