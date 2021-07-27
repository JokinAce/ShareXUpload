namespace ShareXUpload.Models {
    public struct ShareXOutput {

        public ShareXOutput(string status, string errorMsg, string url = "") {
            this.Status = status;
            this.Errormsg = errorMsg;
            this.Url = url;
        }

        public string Status { get; }
        public string Errormsg { get; }
        public string Url { get; }
    }
}