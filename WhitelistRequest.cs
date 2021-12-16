namespace QCS {
    public  class WhiteListRequest{ 

        public String Username {get;set;}
        public String Email {get;set;}
        public WhiteListRequest(String username, String qcsEmail) {
            this.Username = username;
            this.Email = qcsEmail;
        }
    }
}