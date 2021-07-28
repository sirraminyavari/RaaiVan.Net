using System;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using RaaiVan.Modules.CoreNetwork;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.Log;

namespace RaaiVan.Modules.Knowledge
{
    class DataProvider
    {
        private static string GetFullyQualifiedName(string name)
        {
            return "[dbo]." + "[KW_" + name + "]"; //'[dbo].' is database owner and 'KW_' is module qualifier
        }

        private static void _parse_knowledge_types(ref IDataReader reader, ref List<KnowledgeType> lstKnowledgeTypes)
        {
            while (reader.Read())
            {
                try
                {
                    KnowledgeType knowledgeType = new KnowledgeType();

                    if (!string.IsNullOrEmpty(reader["KnowledgeTypeID"].ToString()))
                        knowledgeType.KnowledgeTypeID = (Guid)reader["KnowledgeTypeID"];
                    if (!string.IsNullOrEmpty(reader["KnowledgeType"].ToString()))
                        knowledgeType.Name = (string)reader["KnowledgeType"];

                    if (!string.IsNullOrEmpty(reader["ScoreScale"].ToString()))
                        knowledgeType.ScoreScale = (int)reader["ScoreScale"];
                    if (!string.IsNullOrEmpty(reader["MinAcceptableScore"].ToString()))
                        knowledgeType.MinAcceptableScore = (double)reader["MinAcceptableScore"];
                    if (!string.IsNullOrEmpty(reader["MinEvaluationsCount"].ToString()))
                        knowledgeType.MinEvaluationsCount = (int)reader["MinEvaluationsCount"];
                    if (!string.IsNullOrEmpty(reader["ConvertEvaluatorsToExperts"].ToString()))
                        knowledgeType.ConvertEvaluatorsToExperts = (bool)reader["ConvertEvaluatorsToExperts"];
                    if (!string.IsNullOrEmpty(reader["EvaluationsEditable"].ToString()))
                        knowledgeType.EvaluationsEditable = (bool)reader["EvaluationsEditable"];
                    if (!string.IsNullOrEmpty(reader["EvaluationsEditableForAdmin"].ToString()))
                        knowledgeType.EvaluationsEditableForAdmin = (bool)reader["EvaluationsEditableForAdmin"];
                    if (!string.IsNullOrEmpty(reader["EvaluationsRemovable"].ToString()))
                        knowledgeType.EvaluationsRemovable = (bool)reader["EvaluationsRemovable"];
                    if (!string.IsNullOrEmpty(reader["UnhideEvaluators"].ToString()))
                        knowledgeType.UnhideEvaluators = (bool)reader["UnhideEvaluators"];
                    if (!string.IsNullOrEmpty(reader["UnhideEvaluations"].ToString()))
                        knowledgeType.UnhideEvaluations = (bool)reader["UnhideEvaluations"];
                    if (!string.IsNullOrEmpty(reader["UnhideNodeCreators"].ToString()))
                        knowledgeType.UnhideNodeCreators = (bool)reader["UnhideNodeCreators"];
                    if (!string.IsNullOrEmpty(reader["TextOptions"].ToString()))
                        knowledgeType.TextOptions = (string)reader["TextOptions"];

                    string strNodeSelectType = string.IsNullOrEmpty(reader["NodeSelectType"].ToString()) ? null :
                        (string)reader["NodeSelectType"];
                    try
                    {
                        if (!string.IsNullOrEmpty(strNodeSelectType)) knowledgeType.NodeSelectType =
                            (KnowledgeNodeSelectType)Enum.Parse(typeof(KnowledgeNodeSelectType), strNodeSelectType);
                    }
                    catch { knowledgeType.NodeSelectType = KnowledgeNodeSelectType.NotSet; }

                    string strEvaluationType = string.IsNullOrEmpty(reader["EvaluationType"].ToString()) ? null :
                        (string)reader["EvaluationType"];
                    try
                    {
                        if (!string.IsNullOrEmpty(strEvaluationType)) knowledgeType.EvaluationType =
                            (KnowledgeEvaluationType)Enum.Parse(typeof(KnowledgeEvaluationType), strEvaluationType);
                    }
                    catch { knowledgeType.EvaluationType = KnowledgeEvaluationType.NotSet; }

                    string strEvaluators = string.IsNullOrEmpty(reader["Evaluators"].ToString()) ? null :
                        (string)reader["Evaluators"];

                    if (!string.IsNullOrEmpty(reader["PreEvaluateByOwner"].ToString()))
                        knowledgeType.PreEvaluateByOwner = (bool)reader["PreEvaluateByOwner"];
                    if (!string.IsNullOrEmpty(reader["ForceEvaluatorsDescribe"].ToString()))
                        knowledgeType.ForceEvaluatorsDescribe = (bool)reader["ForceEvaluatorsDescribe"];

                    try
                    {
                        if (!string.IsNullOrEmpty(strEvaluators)) knowledgeType.Evaluators =
                            (KnowledgeEvaluators)Enum.Parse(typeof(KnowledgeEvaluators), strEvaluators);
                    }
                    catch { knowledgeType.Evaluators = KnowledgeEvaluators.NotSet; }

                    string strSearchableAfter = string.IsNullOrEmpty(reader["SearchableAfter"].ToString()) ? null :
                        (string)reader["SearchableAfter"];
                    try
                    {
                        if (!string.IsNullOrEmpty(strSearchableAfter))
                            knowledgeType.SearchableAfter = (SearchableAfter)Enum.Parse(typeof(SearchableAfter), strSearchableAfter);
                    }
                    catch { knowledgeType.SearchableAfter = SearchableAfter.NotSet; }

                    if (string.IsNullOrEmpty(reader["AdditionalIDPattern"].ToString()))
                        knowledgeType.AdditionalIDPattern = CNUtilities.DefaultAdditionalIDPattern;
                    else
                        knowledgeType.AdditionalIDPattern = (string)reader["AdditionalIDPattern"];

                    lstKnowledgeTypes.Add(knowledgeType);
                }
                catch(Exception ex) { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_questions(ref IDataReader reader, ref List<KnowledgeTypeQuestion> lstQuestions)
        {
            while (reader.Read())
            {
                try
                {
                    KnowledgeTypeQuestion question = new KnowledgeTypeQuestion();

                    if (!string.IsNullOrEmpty(reader["ID"].ToString())) question.ID = (Guid)reader["ID"];
                    if (!string.IsNullOrEmpty(reader["KnowledgeTypeID"].ToString()))
                        question.KnowledgeTypeID = (Guid)reader["KnowledgeTypeID"];
                    if (!string.IsNullOrEmpty(reader["QuestionID"].ToString())) question.QuestionID = (Guid)reader["QuestionID"];
                    if (!string.IsNullOrEmpty(reader["QuestionBody"].ToString())) question.QuestionBody = (string)reader["QuestionBody"];
                    if (!string.IsNullOrEmpty(reader["Weight"].ToString())) question.Weight = (double)reader["Weight"];
                    if (!string.IsNullOrEmpty(reader["RelatedNodeID"].ToString()))
                        question.RelatedNode.NodeID = (Guid)reader["RelatedNodeID"];
                    if (!string.IsNullOrEmpty(reader["RelatedNodeName"].ToString()))
                        question.RelatedNode.Name = (string)reader["RelatedNodeName"];

                    lstQuestions.Add(question);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_answers(ref IDataReader reader, ref List<EvaluationAnswer> lstAnswers)
        {
            while (reader.Read())
            {
                try
                {
                    EvaluationAnswer ans = new EvaluationAnswer();

                    if (!string.IsNullOrEmpty(reader["QuestionID"].ToString())) ans.QuestionID = (Guid)reader["QuestionID"];
                    if (!string.IsNullOrEmpty(reader["Title"].ToString())) ans.Title = (string)reader["Title"];
                    if (!string.IsNullOrEmpty(reader["TextValue"].ToString())) ans.TextValue = (string)reader["TextValue"];
                    if (!string.IsNullOrEmpty(reader["Score"].ToString())) ans.Score = (double)reader["Score"];
                    if (!string.IsNullOrEmpty(reader["EvaluationDate"].ToString()))
                        ans.EvaluationDate = (DateTime)reader["EvaluationDate"];

                    lstAnswers.Add(ans);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_answer_options(ref IDataReader reader, ref List<AnswerOption> lstOptions)
        {
            while (reader.Read())
            {
                try
                {
                    AnswerOption option = new AnswerOption();

                    if (!string.IsNullOrEmpty(reader["ID"].ToString())) option.ID = (Guid)reader["ID"];
                    if (!string.IsNullOrEmpty(reader["TypeQuestionID"].ToString()))
                        option.TypeQuestionID = (Guid)reader["TypeQuestionID"];
                    if (!string.IsNullOrEmpty(reader["Title"].ToString())) option.Title = (string)reader["Title"];
                    if (!string.IsNullOrEmpty(reader["Value"].ToString())) option.Value = (double)reader["Value"];

                    lstOptions.Add(option);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_knowledge_admins(ref IDataReader reader, ref List<KeyValuePair<Guid, Guid>> lstResults)
        {
            while (reader.Read())
            {
                try
                {
                    Guid knowledgeId = string.IsNullOrEmpty(reader["KnowledgeID"].ToString()) ? Guid.Empty : (Guid)reader["KnowledgeID"];
                    Guid userId = string.IsNullOrEmpty(reader["UserID"].ToString()) ? Guid.Empty : (Guid)reader["UserID"];

                    if (userId != Guid.Empty && knowledgeId != Guid.Empty) lstResults.Add(new KeyValuePair<Guid, Guid>(knowledgeId, userId));
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_save_evaluation_form(ref IDataReader reader, ref List<Dashboard> retDashboards,
            ref bool result, ref bool accepted, ref bool searchabilityActivated, ref string status)
        {
            result = false;

            RVDataTable tbl = new RVDataTable();

            if (!ProviderUtil.parse_dashboards(ref reader, ref retDashboards, ref tbl)) return;

            result = tbl.Rows[0].Field<int>(tbl.Columns["Result"]) > 0;
            accepted = tbl.Rows[0].Field<bool>(tbl.Columns["Accepted"]);
            searchabilityActivated = tbl.Rows[0].Field<bool>(tbl.Columns["SearchabilityActivated"]);
            status = tbl.Rows[0].Field<string>(tbl.Columns["Status"]);
        }

        private static void _parse_evaluations(ref IDataReader reader, ref List<KnowledgeEvaluation> lstEvaluations)
        {
            while (reader.Read())
            {
                try
                {
                    KnowledgeEvaluation evaluation = new KnowledgeEvaluation();

                    if (!string.IsNullOrEmpty(reader["UserID"].ToString())) evaluation.Evaluator.UserID = (Guid)reader["UserID"];
                    if (!string.IsNullOrEmpty(reader["UserName"].ToString())) evaluation.Evaluator.UserName = (string)reader["UserName"];
                    if (!string.IsNullOrEmpty(reader["FirstName"].ToString())) evaluation.Evaluator.FirstName = (string)reader["FirstName"];
                    if (!string.IsNullOrEmpty(reader["LastName"].ToString())) evaluation.Evaluator.LastName = (string)reader["LastName"];
                    if (!string.IsNullOrEmpty(reader["Score"].ToString())) evaluation.Score = (double)reader["Score"];
                    if (!string.IsNullOrEmpty(reader["EvaluationDate"].ToString())) evaluation.EvaluationDate = (DateTime)reader["EvaluationDate"];
                    if (!string.IsNullOrEmpty(reader["WFVersionID"].ToString())) evaluation.WFVersionID = (int)reader["WFVersionID"];

                    lstEvaluations.Add(evaluation);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_history(ref IDataReader reader, ref List<KWFHistory> lstHistory)
        {
            while (reader.Read())
            {
                try
                {
                    KWFHistory hist = new KWFHistory();
                    
                    if (!string.IsNullOrEmpty(reader["ID"].ToString())) hist.ID = (long)reader["ID"];
                    if (!string.IsNullOrEmpty(reader["KnowledgeID"].ToString())) hist.KnowledgeID = (Guid)reader["KnowledgeID"];
                    if (!string.IsNullOrEmpty(reader["Action"].ToString())) hist.Action = (string)reader["Action"];
                    if (!string.IsNullOrEmpty(reader["TextOptions"].ToString()))
                        hist.TextOptions = ((string)reader["TextOptions"])
                            .Split('~').Select(u => u.Trim()).Where(u => !string.IsNullOrEmpty(u)).ToList();
                    if (!string.IsNullOrEmpty(reader["Description"].ToString()))
                        hist.Description = (string)reader["Description"];
                    if (!string.IsNullOrEmpty(reader["ActorUserID"].ToString()))
                        hist.Actor.UserID = (Guid)reader["ActorUserID"];
                    if (!string.IsNullOrEmpty(reader["ActorUserName"].ToString()))
                        hist.Actor.UserName = (string)reader["ActorUserName"];
                    if (!string.IsNullOrEmpty(reader["ActorFirstName"].ToString()))
                        hist.Actor.FirstName = (string)reader["ActorFirstName"];
                    if (!string.IsNullOrEmpty(reader["ActorLastName"].ToString()))
                        hist.Actor.LastName = (string)reader["ActorLastName"];
                    if (!string.IsNullOrEmpty(reader["DeputyUserID"].ToString()))
                        hist.Deputy.UserID = (Guid)reader["DeputyUserID"];
                    if (!string.IsNullOrEmpty(reader["DeputyUserName"].ToString()))
                        hist.Deputy.UserName = (string)reader["DeputyUserName"];
                    if (!string.IsNullOrEmpty(reader["DeputyFirstName"].ToString()))
                        hist.Deputy.FirstName = (string)reader["DeputyFirstName"];
                    if (!string.IsNullOrEmpty(reader["DeputyLastName"].ToString()))
                        hist.Deputy.LastName = (string)reader["DeputyLastName"];
                    if (!string.IsNullOrEmpty(reader["ActionDate"].ToString()))
                        hist.ActionDate = (DateTime)reader["ActionDate"];
                    if (!string.IsNullOrEmpty(reader["ReplyToHistoryID"].ToString()))
                        hist.ReplyToHistoryID = (long)reader["ReplyToHistoryID"];
                    if (!string.IsNullOrEmpty(reader["WFVersionID"].ToString()))
                        hist.WFVersionID = (int)reader["WFVersionID"];
                    if (!string.IsNullOrEmpty(reader["IsCreator"].ToString()))
                        hist.IsCreator = (bool)reader["IsCreator"];
                    if (!string.IsNullOrEmpty(reader["IsContributor"].ToString()))
                        hist.IsContributor = (bool)reader["IsContributor"];

                    lstHistory.Add(hist);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_feedbacks(ref IDataReader reader, ref List<FeedBack> lstFeedBacks)
        {
            while (reader.Read())
            {
                try
                {
                    FeedBack feedback = new FeedBack();

                    if (!string.IsNullOrEmpty(reader["FeedBackID"].ToString())) feedback.FeedBackID = (long)reader["FeedBackID"];
                    if (!string.IsNullOrEmpty(reader["KnowledgeID"].ToString())) feedback.KnowledgeID = (Guid)reader["KnowledgeID"];
                    if (!string.IsNullOrEmpty(reader["FeedBackTypeID"].ToString()))
                    {
                        int ftypeid = (int)reader["FeedBackTypeID"];
                        feedback.FeedBackType = ftypeid == 1 ? FeedBackTypes.Financial : FeedBackTypes.Temporal;
                    }
                    if (!string.IsNullOrEmpty(reader["SendDate"].ToString())) feedback.SendDate = (DateTime)reader["SendDate"];
                    if (!string.IsNullOrEmpty(reader["Value"].ToString())) feedback.Value = (double)reader["Value"];
                    if (!string.IsNullOrEmpty(reader["Description"].ToString())) feedback.Description = (string)reader["Description"];

                    if (!string.IsNullOrEmpty(reader["UserID"].ToString())) feedback.User.UserID = (Guid)reader["UserID"];
                    if (!string.IsNullOrEmpty(reader["UserName"].ToString())) feedback.User.UserName = (string)reader["UserName"];
                    if (!string.IsNullOrEmpty(reader["FirstName"].ToString())) feedback.User.FirstName = (string)reader["FirstName"];
                    if (!string.IsNullOrEmpty(reader["LastName"].ToString())) feedback.User.LastName = (string)reader["LastName"];

                    lstFeedBacks.Add(feedback);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static void _parse_feedback_status(ref IDataReader reader, ref double totalFinancialFeedbacks,
            ref double totalTemporalFeedbacks, ref double financialFeedbackStatus, ref double temporalFeedbackStatus)
        {
            if (reader.Read())
            {
                try
                {
                    if (!string.IsNullOrEmpty(reader["TotalFinancialFeedBacks"].ToString()))
                        totalFinancialFeedbacks = (double)reader["TotalFinancialFeedBacks"];
                    if (!string.IsNullOrEmpty(reader["TotalTemporalFeedBacks"].ToString()))
                        totalTemporalFeedbacks = (double)reader["TotalTemporalFeedBacks"];
                    if (!string.IsNullOrEmpty(reader["FinancialFeedBackStatus"].ToString()))
                        financialFeedbackStatus = (double)reader["FinancialFeedBackStatus"];
                    if (!string.IsNullOrEmpty(reader["TemporalFeedBackStatus"].ToString()))
                        temporalFeedbackStatus = (double)reader["TemporalFeedBackStatus"];
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();
        }

        private static List<NecessaryItem> _parse_necessary_items(ref IDataReader reader)
        {
            List<NecessaryItem> retItems = new List<NecessaryItem>();

            while (reader.Read())
            {
                try
                {
                    NecessaryItem itm = NecessaryItem.Abstract;

                    if (!string.IsNullOrEmpty(reader["ItemName"].ToString()) &&
                        Enum.TryParse<NecessaryItem>(reader["ItemName"].ToString(), out itm)) retItems.Add(itm);
                }
                catch { }
            }

            if (!reader.IsClosed) reader.Close();

            return retItems;
        }

        public static bool Initialize(Guid applicationId)
        {
            string spName = GetFullyQualifiedName("Initialize");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static bool AddKnowledgeType(Guid applicationId, Guid knowledgeTypeId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("AddKnowledgeType");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    knowledgeTypeId, currentUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static bool ArithmeticDeleteKnowledgeType(Guid applicationId, Guid knowledgeTypeId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeleteKnowledgeType");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    knowledgeTypeId, currentUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static void GetKnowledgeTypes(Guid applicationId, 
            ref List<KnowledgeType> retKnowledgeTypes, ref List<Guid> knowledgeTypeIds)
        {
            string spName = GetFullyQualifiedName("GetKnowledgeTypes");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ref knowledgeTypeIds), ',');
                _parse_knowledge_types(ref reader, ref retKnowledgeTypes);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
            }
        }

        public static bool SetEvaluationType(Guid applicationId, 
            Guid knowledgeTypeId, KnowledgeEvaluationType evaluationType)
        {
            string spName = GetFullyQualifiedName("SetEvaluationType");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, knowledgeTypeId, 
                    evaluationType == KnowledgeEvaluationType.NotSet ? string.Empty : evaluationType.ToString()));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static bool SetEvaluators(Guid applicationId, 
            Guid knowledgeTypeId, KnowledgeEvaluators evaluators, int? minEvaluationsCount)
        {
            string spName = GetFullyQualifiedName("SetEvaluators");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, knowledgeTypeId, 
                    evaluators == KnowledgeEvaluators.NotSet ? string.Empty : evaluators.ToString(), minEvaluationsCount));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static bool SetPreEvaluateByOwner(Guid applicationId, Guid knowledgeTypeId, bool value)
        {
            string spName = GetFullyQualifiedName("SetPreEvaluateByOwner");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, knowledgeTypeId, value));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static bool SetForceEvaluatorsDescribe(Guid applicationId, Guid knowledgeTypeId, bool value)
        {
            string spName = GetFullyQualifiedName("SetForceEvaluatorsDescribe");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, knowledgeTypeId, value));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static bool SetNodeSelectType(Guid applicationId, 
            Guid knowledgeTypeId, KnowledgeNodeSelectType nodeSelectType)
        {
            string spName = GetFullyQualifiedName("SetNodeSelectType");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, knowledgeTypeId, 
                    nodeSelectType == KnowledgeNodeSelectType.NotSet ? string.Empty : nodeSelectType.ToString()));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static bool SetSubmissionType(Guid applicationId, Guid knowledgeTypeId, SubmissionType submissionType)
        {
            string spName = GetFullyQualifiedName("SetSubmissionType");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, knowledgeTypeId, 
                    submissionType == SubmissionType.NotSet ? string.Empty : submissionType.ToString()));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static bool SetSearchabilityType(Guid applicationId, 
            Guid knowledgeTypeId, SearchableAfter searchableAfter)
        {
            string spName = GetFullyQualifiedName("SetSearchabilityType");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, knowledgeTypeId, 
                    searchableAfter == SearchableAfter.NotSet ? string.Empty : searchableAfter.ToString()));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static bool SetScoreScale(Guid applicationId, Guid knowledgeTypeId, int? scoreScale)
        {
            string spName = GetFullyQualifiedName("SetScoreScale");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    knowledgeTypeId, scoreScale));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static bool SetMinAcceptableScore(Guid applicationId, Guid knowledgeTypeId, double? minAcceptableScore)
        {
            string spName = GetFullyQualifiedName("SetMinAcceptableScore");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    knowledgeTypeId, minAcceptableScore));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }
        
        public static bool SetConvertEvaluatorsToExperts(Guid applicationId, Guid knowledgeTypeId, bool value)
        {
            string spName = GetFullyQualifiedName("SetConvertEvaluatorsToExperts");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    knowledgeTypeId, value));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static bool SetEvaluationsEditable(Guid applicationId, Guid knowledgeTypeId, bool value)
        {
            string spName = GetFullyQualifiedName("SetEvaluationsEditable");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    knowledgeTypeId, value));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static bool SetEvaluationsEditableForAdmin(Guid applicationId, Guid knowledgeTypeId, bool value)
        {
            string spName = GetFullyQualifiedName("SetEvaluationsEditableForAdmin");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    knowledgeTypeId, value));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static bool SetEvaluationsRemovable(Guid applicationId, Guid knowledgeTypeId, bool value)
        {
            string spName = GetFullyQualifiedName("SetEvaluationsRemovable");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    knowledgeTypeId, value));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static bool SetUnhideEvaluators(Guid applicationId, Guid knowledgeTypeId, bool value)
        {
            string spName = GetFullyQualifiedName("SetUnhideEvaluators");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    knowledgeTypeId, value));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static bool SetUnhideEvaluations(Guid applicationId, Guid knowledgeTypeId, bool value)
        {
            string spName = GetFullyQualifiedName("SetUnhideEvaluations");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    knowledgeTypeId, value));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static bool SetUnhideNodeCreators(Guid applicationId, Guid knowledgeTypeId, bool value)
        {
            string spName = GetFullyQualifiedName("SetUnhideNodeCreators");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    knowledgeTypeId, value));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static bool SetTextOptions(Guid applicationId, Guid knowledgeTypeId, string value)
        {
            string spName = GetFullyQualifiedName("SetTextOptions");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, knowledgeTypeId, value));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static bool SetCandidateRelations(Guid applicationId, 
            Guid knowledgeTypeId, ref List<Guid> nodeTypeIds, ref List<Guid> nodeIds, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("SetCandidateRelations");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    knowledgeTypeId, ProviderUtil.list_to_string<Guid>(ref nodeTypeIds),
                    ProviderUtil.list_to_string<Guid>(ref nodeIds), ',', currentUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static void GetCandidateNodeRelations(Guid applicationId, 
            ref List<Node> retNodes, Guid knowledgeTypeIdOrKnowledgeId)
        {
            string spName = GetFullyQualifiedName("GetCandidateNodeRelationIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, knowledgeTypeIdOrKnowledgeId);
                List<Guid> nodeIds = new List<Guid>();
                ProviderUtil.parse_guids(ref reader, ref nodeIds);
                retNodes = CNController.get_nodes(applicationId, nodeIds, full: null, currentUserId: null);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
            }
        }

        public static void GetCandidateNodeTypeRelations(Guid applicationId, 
            ref List<NodeType> retNodeTypes, Guid knowledgeTypeIdOrKnowledgeId)
        {
            string spName = GetFullyQualifiedName("GetCandidateNodeTypeRelationIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, knowledgeTypeIdOrKnowledgeId);
                List<Guid> nodeTypeIds = new List<Guid>();
                ProviderUtil.parse_guids(ref reader, ref nodeTypeIds);
                retNodeTypes = CNController.get_node_types(applicationId, nodeTypeIds);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
            }
        }

        public static bool AddQuestion(Guid applicationId, KnowledgeTypeQuestion question, ref string errorMessage)
        {
            string spName = GetFullyQualifiedName("AddQuestion");

            try
            {
                if (question.RelatedNode.NodeID == Guid.Empty) question.RelatedNode.NodeID = null;
                if (!question.CreationDate.HasValue) question.CreationDate = DateTime.Now;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    question.ID, question.KnowledgeTypeID, question.RelatedNode.NodeID, question.QuestionBody,
                    question.Creator.UserID, question.CreationDate), ref errorMessage);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static bool ModifyQuestion(Guid applicationId, KnowledgeTypeQuestion question)
        {
            string spName = GetFullyQualifiedName("ModifyQuestion");

            try
            {
                if (!question.LastModificationDate.HasValue) question.LastModificationDate = DateTime.Now;

                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    question.ID, question.QuestionBody, question.LastModifier.UserID, question.LastModificationDate));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static bool SetQuestionsOrder(Guid applicationId, List<Guid> ids)
        {
            string spName = GetFullyQualifiedName("SetQuestionsOrder");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ids), ','));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }
        
        public static bool SetQuestionWeight(Guid applicationId, Guid id, double weight, ref string errorMessage)
        {
            string spName = GetFullyQualifiedName("SetQuestionWeight");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    id, weight), ref errorMessage);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static bool ArithmeticDeleteQuestion(Guid applicationId, Guid id, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeleteQuestion");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    id, currentUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static bool ArithmeticDeleteRelatedNodeQuestions(Guid applicationId, 
            Guid knowledgeTypeId, Guid nodeId, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeleteRelatedNodeQuestions");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    knowledgeTypeId, nodeId, currentUserId, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static bool AddAnswerOption(Guid applicationId, Guid id, 
            Guid typeQuestionId, string title, double value, Guid currentUserId, ref string errorMessage)
        {
            string spName = GetFullyQualifiedName("AddAnswerOption");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    id, typeQuestionId, title, value, currentUserId, DateTime.Now), ref errorMessage);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static bool ModifyAnswerOption(Guid applicationId, Guid id,
            string title, double value, Guid currentUserId, ref string errorMessage)
        {
            string spName = GetFullyQualifiedName("ModifyAnswerOption");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    id, title, value, currentUserId, DateTime.Now), ref errorMessage);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static bool SetAnswerOptionsOrder(Guid applicationId, List<Guid> ids)
        {
            string spName = GetFullyQualifiedName("SetAnswerOptionsOrder");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(ids), ','));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static bool RemoveAnswerOption(Guid applicationId, Guid id, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeleteAnswerOption");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, 
                    id, currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static void GetQuestions(Guid applicationId, 
            ref List<KnowledgeTypeQuestion> retQuestions, Guid knowledgeTypeId)
        {
            string spName = GetFullyQualifiedName("GetQuestions");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, knowledgeTypeId);
                _parse_questions(ref reader, ref retQuestions);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
            }
        }

        public static void SearchQuestions(Guid applicationId, 
            ref List<string> retQuestions, string searchText, int? count)
        {
            string spName = GetFullyQualifiedName("SearchQuestions");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.get_search_text(searchText), count);
                ProviderUtil.parse_strings(ref reader, ref retQuestions);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
            }
        }

        public static void GetAnswerOptions(Guid applicationId,
            ref List<AnswerOption> retOptions, List<Guid> typeQuestionIds)
        {
            string spName = GetFullyQualifiedName("GetAnswerOptions");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<Guid>(typeQuestionIds), ',');
                _parse_answer_options(ref reader, ref retOptions);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
            }
        }

        public static void GetFilledEvaluationForm(Guid applicationId,
            ref List<EvaluationAnswer> retOptions, Guid knowledgeId, Guid userId, int? wfVersionId)
        {
            string spName = GetFullyQualifiedName("GetFilledEvaluationForm");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, knowledgeId, userId, wfVersionId);
                _parse_answers(ref reader, ref retOptions);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
            }
        }

        public static void GetEvaluationsDone(Guid applicationId, 
            ref List<KnowledgeEvaluation> retEvaluations, Guid knowledgeId, int? wfVersionId)
        {
            string spName = GetFullyQualifiedName("GetEvaluationsDone");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, knowledgeId, wfVersionId);
                _parse_evaluations(ref reader, ref retEvaluations);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
            }
        }

        public static bool HasEvaluated(Guid applicationId, Guid knowledgeId, Guid userId)
        {
            string spName = GetFullyQualifiedName("HasEvaluated");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, knowledgeId, userId));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static bool AcceptRejectKnowledge(Guid applicationId, 
            Guid nodeId, Guid currentUserId, bool accept, List<string> textOptions, string description)
        {
            string spName = GetFullyQualifiedName("AcceptRejectKnowledge");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    nodeId, currentUserId, accept, string.Join(" ~ ", textOptions), description, DateTime.Now));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static bool SendToAdmin(Guid applicationId, Guid nodeId, List<Guid> adminUserIds, 
            Guid currentUserId, string description, ref List<Dashboard> dashboards, ref string message)
        {
            string spName = GetFullyQualifiedName("SendToAdmin");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, nodeId, currentUserId, 
                    ProviderUtil.list_to_string<Guid>(adminUserIds), ',', description, DateTime.Now);

                return ProviderUtil.parse_dashboards(ref reader, ref dashboards, ref message) > 0;
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static bool SendBackForRevision(Guid applicationId, Guid nodeId, Guid currentUserId, 
            List<string> textOptions, string description, ref List<Dashboard> dashboards, ref string message)
        {
            string spName = GetFullyQualifiedName("SendBackForRevision");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    nodeId, currentUserId, string.Join(" ~ ", textOptions), description, DateTime.Now);

                return ProviderUtil.parse_dashboards(ref reader, ref dashboards, ref message) > 0;
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static bool SendToEvaluators(Guid applicationId, Guid nodeId, List<Guid> evaluatorUserIds, 
            Guid currentUserId, string description, ref List<Dashboard> dashboards, ref string message)
        {
            string spName = GetFullyQualifiedName("SendToEvaluators");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, nodeId, currentUserId, 
                    ProviderUtil.list_to_string<Guid>(evaluatorUserIds), ',', description, DateTime.Now);

                return ProviderUtil.parse_dashboards(ref reader, ref dashboards, ref message) > 0;
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static bool NewEvaluators(Guid applicationId, Guid nodeId, List<Guid> evaluatorUserIds, 
            ref List<Dashboard> dashboards, ref string message)
        {
            string spName = GetFullyQualifiedName("NewEvaluators");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    nodeId, ProviderUtil.list_to_string<Guid>(evaluatorUserIds), ',', DateTime.Now);

                return ProviderUtil.parse_dashboards(ref reader, ref dashboards, ref message) > 0;
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static bool SendKnowledgeComment(Guid applicationId, Guid nodeId, Guid userId, long? replyToHistoryId,
            List<Guid> adminUserIds, string description, ref List<Dashboard> retDashboards)
        {
            string spName = GetFullyQualifiedName("SendKnowledgeComment");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    nodeId, userId, replyToHistoryId, string.Join(",", adminUserIds), ',', description, DateTime.Now);

                return ProviderUtil.parse_dashboards(ref reader, ref retDashboards) > 0;
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static bool SaveEvaluationForm(Guid applicationId, Guid nodeId, Guid userId, List<KeyValuePair<Guid, double>> answers, 
            Guid currentUserId, double score, List<Guid> adminUserIds, List<string> textOptions, string description, 
            ref List<Dashboard> retDashboards, ref string status, ref bool searchabilityActivated)
        {
            if (userId == Guid.Empty) userId = currentUserId;

            SqlConnection con = new SqlConnection(ProviderUtil.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //create answers param
            DataTable answersTable = new DataTable();
            answersTable.Columns.Add("FirstValue", typeof(Guid));
            answersTable.Columns.Add("SecondValue", typeof(double));
            foreach (KeyValuePair<Guid, double> _pair in answers)
                answersTable.Rows.Add(_pair.Key, _pair.Value);

            SqlParameter answersParam = new SqlParameter("@Answers", SqlDbType.Structured);
            answersParam.TypeName = "[dbo].[GuidFloatTableType]";
            answersParam.Value = answersTable;
            //end of create answers param

            //create admins param
            DataTable adminsTable = new DataTable();
            adminsTable.Columns.Add("Value", typeof(Guid));
            foreach (Guid uId in adminUserIds)
                adminsTable.Rows.Add(uId);

            SqlParameter adminsParam = new SqlParameter("@AdminUserIDs", SqlDbType.Structured);
            adminsParam.TypeName = "[dbo].[GuidTableType]";
            adminsParam.Value = adminsTable;
            //end of create admins param

            cmd.Parameters.AddWithValue("@ApplicationID", applicationId);
            cmd.Parameters.AddWithValue("@NodeID", nodeId);
            cmd.Parameters.AddWithValue("@UserID", userId);
            cmd.Parameters.AddWithValue("@CurrentUserID", currentUserId);
            cmd.Parameters.Add(answersParam);
            cmd.Parameters.AddWithValue("@Score", score);
            cmd.Parameters.AddWithValue("@EvaluationDate", DateTime.Now);
            cmd.Parameters.Add(adminsParam);
            if(textOptions != null && textOptions.Count > 0) cmd.Parameters.AddWithValue("@TextOptions", string.Join(" ~ ", textOptions));
            cmd.Parameters.AddWithValue("@Description", description);

            string spName = GetFullyQualifiedName("SaveEvaluationForm");

            string sep = ",";
            string arguments = "@ApplicationID" + sep + "@NodeID" + sep + "@UserID" + sep + "@CurrentUserID" + sep + 
                "@Answers" + sep + "@Score" + sep + "@EvaluationDate" + sep + "@AdminUserIDs" + sep + 
                (textOptions == null || textOptions.Count == 0 ? "null" : "@TextOptions") + sep +
                "@Description";
            cmd.CommandText = ("EXEC" + " " + spName + " " + arguments);

            con.Open();

            bool result = !(searchabilityActivated = false); //--> result: true, accepted: false, searchabilityActivated: false

            try
            {
                IDataReader reader = (IDataReader)cmd.ExecuteReader();

                bool accepted = false;

                _parse_save_evaluation_form(ref reader, ref retDashboards, ref result, 
                    ref accepted, ref searchabilityActivated, ref status);
                return result;
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
            finally { con.Close(); }
        }

        public static bool RemoveEvaluator(Guid applicationId, Guid nodeId, Guid userId)
        {
            string spName = GetFullyQualifiedName("RemoveEvaluator");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, nodeId, userId));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static bool RefuseEvaluation(Guid applicationId, Guid nodeId, Guid userId, 
            List<Guid> adminUserIds, string description, ref List<Dashboard> retDashboards)
        {
            string spName = GetFullyQualifiedName("RefuseEvaluation");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    nodeId, userId, DateTime.Now, ProviderUtil.list_to_string<Guid>(adminUserIds), ',', description);
                return ProviderUtil.parse_dashboards(ref reader, ref retDashboards) > 0;
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static bool TerminateEvaluation(Guid applicationId, Guid nodeId, Guid currentUserId, 
            string description, ref bool accepted, ref bool searchabilityActivated)
        {
            string spName = GetFullyQualifiedName("TerminateEvaluation");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    nodeId, currentUserId, description, DateTime.Now);
                bool result = false;

                RVDataTable tbl = new RVDataTable();
                ProviderUtil.reader2table(ref reader, ref tbl, true);

                result = tbl.Rows[0].Field<int>(tbl.Columns["Result"]) > 0;
                accepted = tbl.Rows[0].Field<bool>(tbl.Columns["Accepted"]);
                searchabilityActivated = tbl.Rows[0].Field<bool>(tbl.Columns["SearchabilityActivated"]);

                return result;
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static int? GetLastHistoryVersionID(Guid applicationId, Guid knowledgeId)
        {
            string spName = GetFullyQualifiedName("GetLastHistoryVersionID");

            try
            {
                int? ret = ProviderUtil.succeed_int(ProviderUtil.execute_reader(spName, applicationId, knowledgeId));
                return ret == 0 ? null : ret;
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return null;
            }
        }

        public static bool EditHistoryDescription(Guid applicationId, long id, string description)
        {
            string spName = GetFullyQualifiedName("EditHistoryDescription");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, id, description));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static void GetHistory(Guid applicationId, ref List<KWFHistory> retHistory, 
            Guid knowledgeId, Guid? userId, string action, int? wfVersionId)
        {
            string spName = GetFullyQualifiedName("GetHistory");

            try
            {
                if (userId == Guid.Empty) userId = null;
                if (string.IsNullOrEmpty(action)) action = null;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, knowledgeId, userId, action, wfVersionId);
                _parse_history(ref reader, ref retHistory);
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
            }
        }

        public static KWFHistory GetHistory(Guid applicationId, long id)
        {
            string spName = GetFullyQualifiedName("GetHistoryByID");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, id);
                List<KWFHistory> lst = new List<KWFHistory>();
                _parse_history(ref reader, ref lst);
                return lst.FirstOrDefault();
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return null;
            }
        }

        public static long AddFeedBack(Guid applicationId, FeedBack Info)
        {
            string spName = GetFullyQualifiedName("AddFeedBack");

            try
            {
                int ftypeId = Info.FeedBackType == FeedBackTypes.Financial ? 1 : 2;

                return ProviderUtil.succeed_long(ProviderUtil.execute_reader(spName, applicationId,
                    Info.KnowledgeID, Info.User.UserID, ftypeId, Info.SendDate, Info.Value, Info.Description));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return -1;
            }
        }

        public static bool ModifyFeedBack(Guid applicationId, FeedBack Info)
        {
            string spName = GetFullyQualifiedName("ModifyFeedBack");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    Info.FeedBackID, Info.Value, Info.Description));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static bool ArithmeticDeleteFeedBack(Guid applicationId, long feedbackId)
        {
            string spName = GetFullyQualifiedName("ArithmeticDeleteFeedBack");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId, feedbackId));
            }
            catch(Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static void GetKnowledgeFeedBacks(Guid applicationId, ref List<FeedBack> retFeedBacks, 
            Guid knowledgeId, Guid? userId, FeedBackTypes? feedbackType, DateTime? sendDateLowerThreshold, 
            DateTime? sendDateUpperThreshold)
        {
            string spName = GetFullyQualifiedName("GetKnowledgeFeedBacks");

            try
            {
                int? feedbackTypeId = null;
                if (feedbackType.HasValue) feedbackTypeId = feedbackType.Value == FeedBackTypes.Financial ? 1 : 2;

                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    knowledgeId, userId, feedbackTypeId, sendDateLowerThreshold, sendDateUpperThreshold);
                _parse_feedbacks(ref reader, ref retFeedBacks);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
            }
        }

        public static void GetKnowledgeFeedBacks(Guid applicationId, 
            ref List<FeedBack> retFeedBacks, ref List<long> feedbackIds)
        {
            string spName = GetFullyQualifiedName("GetFeedBacksByIDs");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId,
                    ProviderUtil.list_to_string<long>(ref feedbackIds), ',');
                _parse_feedbacks(ref reader, ref retFeedBacks);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
            }
        }

        public static void GetFeedBackStatus(Guid applicationId, Guid knowledgeId, Guid? userId, 
            ref double totalFinancialFeedbacks, ref double totalTemporalFeedbacks, 
            ref double financialFeedbackStatus, ref double temporalFeedbackStatus)
        {
            string spName = GetFullyQualifiedName("GetFeedBackStatus");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, knowledgeId, userId);
                _parse_feedback_status(ref reader, ref totalFinancialFeedbacks, ref totalTemporalFeedbacks,
                    ref financialFeedbackStatus, ref temporalFeedbackStatus);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
            }
        }

        public static List<NecessaryItem> GetNecessaryItems(Guid applicationId, Guid nodeTypeIdOrNodeId)
        {
            string spName = GetFullyQualifiedName("GetNecessaryItems");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, nodeTypeIdOrNodeId);
                return _parse_necessary_items(ref reader);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return new List<NecessaryItem>();
            }
        }

        public static bool ActivateNecessaryItem(Guid applicationId, Guid knowledgeTypeId, 
            NecessaryItem itm, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("ActivateNecessaryItem");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    knowledgeTypeId, itm.ToString(), currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static bool DeactiveNecessaryItem(Guid applicationId, Guid knowledgeTypeId,
            NecessaryItem itm, Guid currentUserId)
        {
            string spName = GetFullyQualifiedName("DeactiveNecessaryItem");

            try
            {
                return ProviderUtil.succeed(ProviderUtil.execute_reader(spName, applicationId,
                    knowledgeTypeId, itm.ToString(), currentUserId, DateTime.Now));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return false;
            }
        }

        public static List<NecessaryItem> CheckNecessaryItems(Guid applicationId, Guid knowledgeId)
        {
            string spName = GetFullyQualifiedName("CheckNecessaryItems");

            try
            {
                IDataReader reader = ProviderUtil.execute_reader(spName, applicationId, knowledgeId);
                return _parse_necessary_items(ref reader);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, spName, ex, ModuleIdentifier.KW);
                return new List<NecessaryItem>();
            }
        }
    }
}
