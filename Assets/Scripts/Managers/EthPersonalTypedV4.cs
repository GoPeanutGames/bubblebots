internal class EthPersonalTypedV4<T>
{
    private string address;
    private object data;
    private object eip712Domain;

    public EthPersonalTypedV4(string address, object data, object eip712Domain)
    {
        this.address = address;
        this.data = data;
        this.eip712Domain = eip712Domain;
    }
}