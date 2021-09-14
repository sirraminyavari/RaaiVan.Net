using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RaaiVan.Modules.FormGenerator;
using RaaiVan.Modules.GlobalUtilities;

namespace RaaiVan.Web.API
{
    public class APOMaturityAssessment
    {
        private static string FormName = "apo_maturity_assessment";

        private static Guid get_poll_app_id(Guid applicationId) {
            Guid baseId = Guid.Parse("2358C791-CC7E-402B-AF31-AF26E84500F8");
            return PublicMethods.guid_xor(baseId, applicationId);
        }

        private static Poll get_poll(Guid applicationId, Guid currentUserId)
        {
            FormType form = FGController.get_form(applicationId, FormName);
            
            if (form == null)
            {
                //create form ******************************************************************************
            }

            if (form == null || !form.FormID.HasValue) return null;

            List<Poll> pollsList = FGController.get_polls_by_form_id(applicationId, formId: form.FormID.Value, archive: null);

            if (pollsList != null && pollsList.Count > 1)
                return null; //there must be exactly one poll using the form
            else if (pollsList != null && pollsList.Count == 1)
            {
                Poll poll = pollsList.FirstOrDefault();

                if (poll.Archived.HasValue && poll.Archived.Value)
                    FGController.recycle_poll(applicationId, poll.PollID.Value, currentUserId);

                return poll;
            }
            else
            {
                Poll poll = new Poll()
                {
                    PollID = Guid.NewGuid(),
                    OwnerID = get_poll_app_id(applicationId),
                    Name = "apo maturity assessment"
                };

                bool result = FGController.add_poll(applicationId, pollId: poll.PollID.Value, copyFromPollId: null,
                    ownerId: poll.OwnerID, name: poll.Name, currentUserId: currentUserId);

                return result ? poll : null;
            }
        }

        private static List<Poll> get_poll_instances(Guid applicationId, Guid pollId, bool archive = false)
        {
            Guid pollAppId = get_poll_app_id(applicationId);

            long totalCount = 0;

            return FGController.get_polls(applicationId, isCopyOfPollId: pollId, ownerId: pollAppId, 
                archive: archive, searchText: null, count: 1000, lowerBoundary: 1000, totalCount: ref totalCount);
        }

        private static string get_statistics(Guid applicationId, Guid currentUserId)
        {
            Poll pollTemplate = get_poll(applicationId, currentUserId);

            if (pollTemplate == null || !pollTemplate.PollID.HasValue) return "{}";

            List<Poll> polls = get_poll_instances(applicationId, pollTemplate.PollID.Value, archive: false);
            
            return "{\"Polls\":[" + string.Join(",", polls.Select(p => {
                FormStatistics stats = FGController.get_form_statistics(applicationId, 
                    ownerId: get_poll_app_id(applicationId), instanceId: null);

                return "{\"Poll\":" + p.toJson() + ",\"Statistics\":" + (stats == null ? "{}" : stats.toJson()) + "}";
            })) + "]}";
        }
    }
}