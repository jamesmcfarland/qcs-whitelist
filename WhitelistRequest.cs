namespace QCS
{
    public class WhiteListRequest
    {

        public String Username { get; set; }
        public String Email { get; set; }

        public String studentNumber { get; set; }
        public WhiteListRequest(String username, String qcsEmail, String studentNumber)
        {
            this.Username = username;
            this.Email = qcsEmail;

            int studentNo;
            if (Int32.TryParse(studentNumber, out studentNo))
            {
                if (studentNo != 0)
                    this.studentNumber = studentNumber;
                else this.studentNumber = "-1";
            }
            else
            {
                this.studentNumber = "-1";
            }

        }

        public bool isSameUser(string usr) {
            return usr==Username;
        }
    }
}