namespace WorkPanel.DataExchange.Responces
{
    public class MetaResponse<T>
    {
        #region Properties

        public int ErrorCode { get; set; }

        public string ErrorMessage { get; set; }

        public string ErrorName { get; set; }

        public T ResponseObject { get; set; }

        public int StatusCode { get; set; }

        #endregion
    }
}
