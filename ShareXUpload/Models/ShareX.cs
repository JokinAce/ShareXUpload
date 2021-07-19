namespace ShareXUpload {

    public class ShareXInput {
        public string Secret { get; set; }
    }

    public struct ShareXOutput {

        public ShareXOutput(string Status, string Errormsg, string Url = "") {
            this.Status = Status;
            this.Errormsg = Errormsg;
            this.Url = Url;
        }

        public string Status { get; set; }
        public string Errormsg { get; set; }
        public string Url { get; set; }
    }
}