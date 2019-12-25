namespace IOCommunation
{
    public struct BitOfBoard
    {
        public readonly int Port;
        public readonly int Bit;
        public BitOfBoard(int port, int bit)
        {
            Port = port;
            Bit = bit;
        }
    }
}
