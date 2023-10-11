using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoGram.Instagram.Response
{
    class MediaCommentResponse : TraitResponse, IResponse
    {
        public Model.Comment Comment;

        public bool IsComment()
        {
            return Comment?.text != null;
        }

        public bool IsSpam()
        {
            return this.Message.Contains("feedback_required");
        }
    }
}
