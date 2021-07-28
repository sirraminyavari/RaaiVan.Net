using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search.Vectorhighlight;
using System.IO;
using System.Web;
using System.Threading;
using RaaiVan.Modules.CoreNetwork;
using RaaiVan.Modules.QA;
using RaaiVan.Modules.Users;
using RaaiVan.Modules.GlobalUtilities;
using RaaiVan.Modules.Privacy;
using RaaiVan.Modules.Log;
using RaaiVan.Modules.Documents;

namespace RaaiVan.Modules.Search
{
    public static class SearchUtilities
    {
        private enum FieldName
        {
            ID,
            TypeID,
            Type,
            AdditionalID,
            Title,
            Description,
            Tags,
            Deleted,
            Content,
            FileContent,
            SearchDocType,
            NoContent
        };

        private static HashSet<string> _StopWords;
        private static HashSet<string> StopWords
        {
            get
            {
                if (_StopWords != null && _StopWords.Count > 0) return _StopWords;

                _StopWords = new HashSet<string>();

                var stopWords = new[]
                {
                    "به","با","از","تا","و","است","هست","هستم","هستیم","هستید","هستند","نیست","نیستم","نیستیم","نیستند","اما","یا",
                    "این","آن","اینجا","آنجا","بود","باد","برای","که","دارم","داری","دارد","داریم","دارید","دارند","چند","را","ها",
                    "های","می","هم","در","باشم","باشی","باشد","باشیم","باشید","باشند","اگر","مگر","بجز","جز","الا","اینکه","چرا","کی",
                    "چه","چطور","چی","چیست","آیا","چنین","اینچنین","نخست","اول","آخر","انتها","صد","هزار","میلیون","ملیون","میلیارد",
                    "ملیارد","یکهزار","تریلیون","تریلیارد","میان","بین","زیر","بیش","روی","ضمن","همانا","ای","بعد","پس","قبل","پیش",
                    "هیچ","همه","واما","شد","شده","شدم","شدی","شدیم","شدند","یک","یکی","نبود","میکند","میکنم","میکنیم","میکنید",
                    "میکنند","میکنی","طور","اینطور","آنطور","هر","حال","مثل","خواهم","خواهی","خواهد","خواهیم","خواهید","خواهند",
                    "داشته","داشت","داشتی","داشتم","داشتیم","داشتید","داشتند","آنکه","مورد","کنید","کنم","کنی","کنند","کنیم",
                    "نکنم","نکنی","نکند","نکنیم","نکنید","نکنند","نکن","بگو","نگو","مگو","بنابراین","بدین","من","تو","او","ما",
                    "شما","ایشان","ی","ـ","هایی","خیلی","بسیار","1","بر","l","شود","کرد","کرده","نیز","خود","شوند","اند","داد","دهد",
                    "گشت","ز","گفت","آمد","اندر","چون","بد","چو","همی","پر","سوی","دو","گر","بی","گرد","زین","کس","زان","جای","آید"
                };

                foreach (var item in stopWords) _StopWords.Add(PublicMethods.verify_string(item));

                return _StopWords;
            }
        }

        private static StandardAnalyzer _STDAnalyzer;
        private static StandardAnalyzer STDAnalyzer
        {
            get
            {
                if (_STDAnalyzer == null) _STDAnalyzer = new StandardAnalyzer(LuceneVersion, StopWords);
                return _STDAnalyzer;
            }
        }

        private static Lucene.Net.Util.Version LuceneVersion
        {
            get { return Lucene.Net.Util.Version.LUCENE_30; }
        }

        private static SortedList<Guid, RAMDirectory> _RamDirs;
        private static RAMDirectory RamDir(Guid applicationId)
        {
            if (_RamDirs == null) _RamDirs = new SortedList<Guid, RAMDirectory>();

            if (!_RamDirs.ContainsKey(applicationId))
            {
                string path = DocFileInfo.index_folder_address(applicationId);
                if (!System.IO.Directory.Exists(path)) System.IO.Directory.CreateDirectory(path);

                FSDirectory dir = FSDirectory.Open(new DirectoryInfo(path));
                _RamDirs.Add(applicationId, new RAMDirectory(dir));
                dir.Dispose();
            }

            return _RamDirs[applicationId];
        }

        private static SortedList<Guid, FSDirectory> _HardDirs;
        private static FSDirectory HardDir(Guid applicationId)
        {
            if (_HardDirs == null) _HardDirs = new SortedList<Guid, FSDirectory>();

            if (!_HardDirs.ContainsKey(applicationId))
            {
                string path = DocFileInfo.index_folder_address(applicationId);
                if (!System.IO.Directory.Exists(path)) System.IO.Directory.CreateDirectory(path);

                FSDirectory dir = FSDirectory.Open(new DirectoryInfo(path));
                _HardDirs.Add(applicationId, dir);
            }

            return _HardDirs[applicationId];
        }

        private static IndexWriter _create_writer(Guid applicationId, bool ram)
        {
            try
            {
                return new IndexWriter(ram ? (Lucene.Net.Store.Directory)RamDir(applicationId) : HardDir(applicationId),
                    STDAnalyzer, create: false, mfl: IndexWriter.MaxFieldLength.UNLIMITED);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, "CreateIndexWriter", ex, ModuleIdentifier.SRCH);

                return new IndexWriter(ram ? (Lucene.Net.Store.Directory)RamDir(applicationId) : HardDir(applicationId),
                        STDAnalyzer, create: true, mfl: IndexWriter.MaxFieldLength.UNLIMITED);
            }
        }

        private static void _close_writer(Guid applicationId, ref IndexWriter writer)
        {
            try
            {
                writer.Optimize();
                writer.Commit();
                writer.Dispose();
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, "CloseIndexWriter", ex, ModuleIdentifier.SRCH);
            }
        }

        private static void _add_docs(Guid applicationId, ref List<SearchDoc> docs, ref IndexWriter writer)
        {
            try
            {
                foreach (SearchDoc doc in docs)
                {
                    if (doc.Type == SearchDocType.All.ToString()) continue;
                    Document d = doc.ToDocument(applicationId);
                    if (d != null) writer.AddDocument(d);
                }
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, "AddIndexDocuments", ex, ModuleIdentifier.SRCH);
            }
        }

        private static void _remove_docs(Guid applicationId, ref List<SearchDoc> docs, ref IndexWriter writer)
        {
            try
            {
                foreach (SearchDoc dc in docs)
                {
                    writer.DeleteDocuments(new Term("ID", dc.ID.ToString()));
                }
                writer.Commit();
                writer.Dispose();
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, "RemoveIndexDocuments", ex, ModuleIdentifier.SRCH);
            }
        }

        private static void _remove_docs(Guid applicationId, List<SearchDoc> docs)
        {
            if (RaaiVanSettings.Solr.Enabled)
            {
                SolrAPI.delete(applicationId, docs);
                return;
            }

            try
            {
                //Delete from Hard
                IndexWriter writer = _create_writer(applicationId, false);
                foreach (SearchDoc dc in docs)
                    writer.DeleteDocuments(new Term("ID", dc.ID.ToString()));
                _close_writer(applicationId, ref writer);

                //Delete from Ram
                if (RaaiVanSettings.IndexUpdate.Ram(applicationId))
                {
                    writer = _create_writer(applicationId, true);
                    foreach (SearchDoc dc in (List<SearchDoc>)docs)
                        writer.DeleteDocuments(new Term("ID", dc.ID.ToString()));
                    _close_writer(applicationId, ref writer);
                }
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, "RemoveIndexDocuments", ex, ModuleIdentifier.SRCH);
            }
        }

        private static void _remove_all_docs(Guid applicationId)
        {
            try
            {
                IndexWriter writer;
                //Delete from Ram
                if (RaaiVanSettings.IndexUpdate.Ram(applicationId))
                {
                    writer = _create_writer(applicationId, true);
                    writer.DeleteAll();
                    _close_writer(applicationId, ref writer);
                }

                //Delete from Hard
                writer = _create_writer(applicationId, false);
                writer.DeleteAll();
                _close_writer(applicationId, ref writer);
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, "RemoveAllIndexDocuments", ex, ModuleIdentifier.SRCH);
            }
        }

        private static void _create_index(Guid applicationId, List<SearchDoc> docs)
        {
            try
            {
                //Write into Hard
                IndexWriter hardWriter = _create_writer(applicationId, false);
                _add_docs(applicationId, ref docs, ref hardWriter);
                _close_writer(applicationId, ref hardWriter);

                //Write into Ram
                if (RaaiVanSettings.IndexUpdate.Ram(applicationId))
                {
                    IndexWriter ramWriter = _create_writer(applicationId, true);
                    _add_docs(applicationId, ref docs, ref ramWriter);
                    _close_writer(applicationId, ref ramWriter);
                }
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, "CreateIndexDocuments", ex, ModuleIdentifier.SRCH);
            }
        }

        private static List<SearchDoc> process_search_results(Guid applicationId, List<SearchDoc> listDocs,
            Guid? currentUserId, ref List<SearchDoc> toBeRemoved, int count)
        {
            List<DocFileInfo> files = new List<DocFileInfo>();

            Dictionary<SearchDocType, List<Guid>> existingObjs = get_existing_ids(applicationId, listDocs, ref files);

            listDocs.Where(doc => files.Any(u => u.FileID == doc.ID)).ToList().ForEach(doc =>
            {
                doc.FileInfo = files.Where(u => u.FileID == doc.ID).FirstOrDefault();
            });

            List<Guid> existingIds = new List<Guid>();

            //Remove not existing docs
            foreach (SearchDoc sd in listDocs)
                if (!existingObjs.Any(x => x.Value.Any(z => z == sd.ID))) toBeRemoved.Add(sd);
            //end of Remove not existing docs

            List<Guid> granted = new List<Guid>();

            //Check access to nodes
            List<Guid> nodeIdsToCheckAccess = new List<Guid>();
            List<Guid> idsToCheckAccess = new List<Guid>();

            if (existingObjs.ContainsKey(SearchDocType.Node)) nodeIdsToCheckAccess.AddRange(existingObjs[SearchDocType.Node]);
            if (existingObjs.ContainsKey(SearchDocType.File))
            {
                existingObjs[SearchDocType.File].ForEach(f =>
                {
                    SearchDoc fl = listDocs.Where(x => x.ID == f && x.FileInfo != null &&
                        x.FileInfo.OwnerNodeID.HasValue).FirstOrDefault();
                    if (fl == null) return;

                    if (!nodeIdsToCheckAccess.Any(a => a == fl.FileInfo.OwnerNodeID))
                        nodeIdsToCheckAccess.Add(fl.FileInfo.OwnerNodeID.Value);

                    if (fl.FileInfo.OwnerID.HasValue && fl.FileInfo.OwnerID != fl.FileInfo.OwnerNodeID)
                    {
                        if (!idsToCheckAccess.Any(a => a == fl.FileInfo.OwnerID))
                            idsToCheckAccess.Add(fl.FileInfo.OwnerID.Value);
                    }

                    if (fl.FileInfo.OwnerID.HasValue && fl.FileInfo.OwnerID != fl.FileInfo.OwnerNodeID &&
                        !idsToCheckAccess.Any(a => a == fl.FileInfo.OwnerID))
                        idsToCheckAccess.Add(fl.FileInfo.OwnerID.Value);
                });
            }

            List<PermissionType> pts = new List<PermissionType>();
            pts.Add(PermissionType.View);
            pts.Add(PermissionType.ViewAbstract);
            pts.Add(PermissionType.Download);

            Dictionary<Guid, List<PermissionType>> ps = PrivacyController.check_access(applicationId,
                currentUserId, nodeIdsToCheckAccess, PrivacyObjectType.Node, pts);

            granted.AddRange(ps.Keys.Where(
                k => ps[k].Any(p => p == PermissionType.ViewAbstract || p == PermissionType.View)));

            List<Guid> grantedFileOwners = PrivacyController.check_access(applicationId,
                currentUserId, idsToCheckAccess, PrivacyObjectType.None, PermissionType.View);

            listDocs.Where(d => d.SearchDocType == SearchDocType.File && d.FileInfo != null &&
                d.FileInfo.OwnerNodeID.HasValue).ToList().ForEach(doc =>
                {
                    Guid ndId = doc.FileInfo.OwnerNodeID.Value;

                    bool isGranted = ps.ContainsKey(ndId) && ps[ndId].Any(u => u == PermissionType.View) &&
                        ps[ndId].Any(u => u == PermissionType.Download);

                    if (isGranted && doc.FileInfo.OwnerID.HasValue && doc.FileInfo.OwnerID != ndId &&
                        !grantedFileOwners.Any(o => o == doc.FileInfo.OwnerID))
                        isGranted = false;

                    doc.AccessIsDenied = !isGranted;
                });
            //end of Check access to nodes

            //Check access to other objects
            List<Guid> ids = new List<Guid>();

            existingObjs.Keys.Where(x => x != SearchDocType.Node).ToList()
                .ForEach(u => ids.AddRange(existingObjs[u]));

            granted.AddRange(PrivacyController.check_access(applicationId,
                currentUserId, ids, PrivacyObjectType.None, PermissionType.View));
            //end of Check access to other objects

            //Check permissions
            bool forceCheckPermission = RaaiVanSettings.IndexUpdate.CheckPermissions(applicationId);

            existingObjs.Keys.ToList().ForEach(k =>
            {
                existingObjs[k].ForEach(id =>
                {
                    SearchDoc doc = listDocs.Where(d => d.ID == id).FirstOrDefault();
                    if (doc == null) return;

                    bool isGranted = doc.AccessIsDenied.HasValue && doc.AccessIsDenied.Value ?
                        false : granted.Any(x => x == id);

                    if (!isGranted) doc.AccessIsDenied = true;

                    if (isGranted || !forceCheckPermission) existingIds.Add(id);
                });
            });
            //end of Check permissions

            return listDocs.Where(doc => existingIds.Any(x => x == doc.ID))
                .Take(Math.Min(count, listDocs.Count)).ToList();
        }

        private static void create_lucene_searcher(Guid applicationId, List<SearchDocType> docTypes, List<Guid> typeIds,
            List<string> types, bool additionalId, bool title, bool description, bool content, bool tages, bool fileContent,
            bool forceHasContent, string phrase, ref Query query, ref IndexSearcher searcher)
        {
            try
            {
                if (string.IsNullOrEmpty(phrase) || phrase.Trim().Length == 0) return;

                phrase = PublicMethods.verify_string(phrase).Replace(":", " ");
                docTypes = docTypes.Distinct().ToList();

                StringBuilder __phrase = new StringBuilder(phrase);

                int curQuot = -1;
                int secondQuot = 0;
                char escapeChar = Convert.ToChar((byte)6);
                while (secondQuot >= 0)
                {
                    curQuot = phrase.IndexOf("\"", secondQuot == 0 ? 0 : secondQuot + 1);
                    secondQuot = curQuot < 0 || phrase.Length == curQuot + 1 ? -1 : phrase.IndexOf("\"", curQuot + 1);

                    if (secondQuot >= 0)
                        for (int i = curQuot; i <= secondQuot; ++i) if (phrase[i] == ' ') __phrase[i] = escapeChar;
                }
                phrase = __phrase.ToString();

                List<string> terms = phrase.Trim().Split(' ').Select(u => u.Replace(escapeChar, ' ').Trim()).ToList();

                phrase = string.Empty;
                float maxBoost = SearchDoc.BoostMax;
                foreach (string str in terms)
                {
                    if (string.IsNullOrEmpty(str) || str == "\"" || str == "\"\"") continue;

                    phrase += (string.IsNullOrEmpty(phrase) ? string.Empty : " ") + str + "^" + maxBoost.ToString();
                    maxBoost -= SearchDoc.BoostStep;
                    if (maxBoost <= SearchDoc.BoostThreshold) maxBoost = SearchDoc.BoostThreshold;
                }

                string strItemTypes = string.Join("^" + SearchDoc.BoostDocType.ToString() + " ", docTypes);
                string strTypeIDs = string.Join("^" + SearchDoc.BoostTypeID.ToString() + " ", typeIds);
                string strTypes = string.Join("^" + SearchDoc.BoostType.ToString() + " ", types);

                List<Occur> FlagesList = new List<Occur>();
                List<string> queries = new List<string>();
                List<string> fields = new List<string>();

                if (!string.IsNullOrEmpty(strItemTypes))
                {
                    fields.Add(FieldName.SearchDocType.ToString());
                    queries.Add(strItemTypes);
                    FlagesList.Add(Occur.MUST);
                }

                if (typeIds != null && !string.IsNullOrEmpty(strTypeIDs))
                {
                    fields.Add(FieldName.TypeID.ToString());
                    queries.Add(strTypeIDs.ToString());
                    FlagesList.Add(Occur.MUST);
                }

                if (types != null && !string.IsNullOrEmpty(strTypes))
                {
                    fields.Add(FieldName.Type.ToString());
                    queries.Add(strTypes.ToString());
                    FlagesList.Add(Occur.MUST);
                }

                if (additionalId)
                {
                    fields.Add(FieldName.AdditionalID.ToString());
                    queries.Add(phrase);
                    FlagesList.Add(Occur.SHOULD);
                }

                if (title)
                {
                    fields.Add(FieldName.Title.ToString());
                    queries.Add(phrase);
                    FlagesList.Add(Occur.SHOULD);
                }

                if (description)
                {
                    fields.Add(FieldName.Description.ToString());
                    queries.Add(phrase);
                    FlagesList.Add(Occur.SHOULD);
                }

                if (content)
                {
                    fields.Add(FieldName.Content.ToString());
                    queries.Add(phrase);
                    FlagesList.Add(Occur.SHOULD);
                }

                if (tages)
                {
                    fields.Add(FieldName.Tags.ToString());
                    queries.Add(phrase);
                    FlagesList.Add(Occur.SHOULD);
                }

                if (fileContent)
                {
                    fields.Add(FieldName.FileContent.ToString());
                    queries.Add(phrase);
                    FlagesList.Add(Occur.SHOULD);
                }

                if (forceHasContent)
                {
                    fields.Add(FieldName.NoContent.ToString());
                    queries.Add(true.ToString().ToLower());
                    FlagesList.Add(Occur.MUST_NOT);
                }

                if (queries.Count == 0 && fields.Count == 0 && FlagesList.Count == 0) return;

                query = MultiFieldQueryParser.Parse(LuceneVersion,
                    queries.ToArray(), fields.ToArray(), FlagesList.ToArray(), STDAnalyzer);

                bool inRam = RaaiVanSettings.IndexUpdate.Ram(applicationId);
                searcher = inRam ? new IndexSearcher(RamDir(applicationId)) : new IndexSearcher(HardDir(applicationId));
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, "CreateLuceneSearcher", ex, ModuleIdentifier.SRCH, LogLevel.Fatal);
            }
        }

        private static List<SearchDoc> lucene_search(Guid applicationId, int lowerBoundary, int count, ref Query query,
            ref IndexSearcher searcher, bool additionalId, bool title, bool description, bool content, bool tags, bool fileContent)
        {
            try
            {
                List<SearchDoc> listDocs = new List<SearchDoc>();

                TopDocs hits = searcher.Search(query, lowerBoundary + count + (count / 2));
                FastVectorHighlighter fvHighlighter = new FastVectorHighlighter(true, true);

                for (int i = lowerBoundary, lnt = hits.ScoreDocs.Length; i < lnt; ++i)
                {
                    ScoreDoc sd = hits.ScoreDocs[i];

                    string addIdFr = !additionalId ? string.Empty :
                        fvHighlighter.GetBestFragment(fvHighlighter.GetFieldQuery(query),
                        searcher.IndexReader, docId: sd.Doc, fieldName: "AdditionalID", fragCharSize: 200);
                    string titleFr = !title ? string.Empty :
                        fvHighlighter.GetBestFragment(fvHighlighter.GetFieldQuery(query),
                        searcher.IndexReader, docId: sd.Doc, fieldName: "Title", fragCharSize: 200);
                    string descFr = !description ? string.Empty :
                        fvHighlighter.GetBestFragment(fvHighlighter.GetFieldQuery(query),
                            searcher.IndexReader, docId: sd.Doc, fieldName: "Description", fragCharSize: 200);
                    string contentFr = !content ? string.Empty :
                        fvHighlighter.GetBestFragment(fvHighlighter.GetFieldQuery(query),
                            searcher.IndexReader, docId: sd.Doc, fieldName: "Content", fragCharSize: 200);
                    string tagsFr = !tags ? string.Empty :
                        fvHighlighter.GetBestFragment(fvHighlighter.GetFieldQuery(query),
                            searcher.IndexReader, docId: sd.Doc, fieldName: "Tags", fragCharSize: 200);
                    string fileFr = !fileContent ? string.Empty :
                        fvHighlighter.GetBestFragment(fvHighlighter.GetFieldQuery(query),
                            searcher.IndexReader, docId: sd.Doc, fieldName: "FileContent", fragCharSize: 200);

                    if (!string.IsNullOrEmpty(titleFr)) titleFr = titleFr.Trim();
                    if (!string.IsNullOrEmpty(addIdFr)) addIdFr = addIdFr.Trim();

                    string highlightedText = ((string.IsNullOrEmpty(descFr) ? string.Empty : descFr + " ") +
                        (string.IsNullOrEmpty(contentFr) ? string.Empty : contentFr + " ") +
                        (string.IsNullOrEmpty(tagsFr) ? string.Empty : tagsFr + " ") +
                        (string.IsNullOrEmpty(fileFr) ? string.Empty : fileFr)).Trim();

                    if (string.IsNullOrEmpty(addIdFr) && string.IsNullOrEmpty(titleFr) && string.IsNullOrEmpty(highlightedText)) break;

                    Document doc = searcher.Doc(sd.Doc);
                    SearchDoc item = SearchDoc.ToSearchDoc(doc);
                    item.Description = highlightedText;
                    listDocs.Add(item);
                }

                return listDocs;
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, "SearchIndexDocuments", ex, ModuleIdentifier.SRCH);
                return new List<SearchDoc>();
            }
        }

        private static void search(Guid applicationId, ref List<SearchDoc> retDocs, ref List<SearchDoc> toBeRemoved,
            Guid? currentUserId, ref int? lowerBoundary, int count, ref Query query, ref IndexSearcher searcher,
            string phrase, List<SearchDocType> docTypes, List<Guid> typeIds, List<string> types, bool additionalId, bool title,
            bool description, bool content, bool tags, bool fileContent, bool forceHasContent, bool highlight, ref int totalCount)
        {
            if (!lowerBoundary.HasValue) lowerBoundary = 0;

            int newBoundary = lowerBoundary.Value;

            List<SearchDoc> listDocs = new List<SearchDoc>();

            if (RaaiVanSettings.Solr.Enabled)
            {
                listDocs = SolrAPI.search(applicationId, phrase, docTypes, typeIds, types, additionalId, title,
                    description, tags, content, fileContent, forceHasContent, count, newBoundary, highlight, ref totalCount)
                    .Select(d => SearchDoc.ToSearchDoc(d)).Where(d => d != null).ToList();
            }
            else
            {
                if (query == null || searcher == null)
                {
                    create_lucene_searcher(applicationId, docTypes, typeIds, types, additionalId,
                        title, description, content, tags, fileContent, forceHasContent, phrase, ref query, ref searcher);

                    if (query == null || searcher == null) return;
                }

                listDocs = lucene_search(applicationId, newBoundary, count, ref query, ref searcher,
                    additionalId, title, description, content, tags, fileContent);
            }

            retDocs.AddRange(process_search_results(applicationId, listDocs, currentUserId, ref toBeRemoved, count));

            newBoundary += listDocs.Count;

            if (lowerBoundary != newBoundary)
            {
                lowerBoundary = newBoundary;
                if (retDocs.Count < count) search(applicationId, ref retDocs, ref toBeRemoved, currentUserId, ref lowerBoundary,
                    count - retDocs.Count, ref query, ref searcher, phrase, docTypes, typeIds, types, additionalId, title,
                    description, content, tags, fileContent, forceHasContent, highlight, ref totalCount);
            }
        }

        public static List<SearchDoc> search(Guid applicationId, List<SearchDocType> docTypes, Guid? currentUserId,
            List<Guid> typeIds, List<string> types, bool additionalId, bool title, bool description, bool content,
            bool tags, bool fileContent, bool forceHasContent, string phrase, ref int? lowerBoundary, int count,
            bool highlight, ref int totalCount)
        {
            if (docTypes == null) docTypes = new List<SearchDocType>();

            additionalId = additionalId &&
                (new[] { SearchDocType.Node, SearchDocType.User, SearchDocType.NodeType }).Any(t => docTypes.Any(d => d == t));
            description = description && docTypes.Any(u => u != SearchDocType.File);
            content = content && (new[] { SearchDocType.Node, SearchDocType.Question }).Any(t => docTypes.Any(d => d == t));
            tags = tags && docTypes.Any(u => u == SearchDocType.Node);
            fileContent = fileContent && (new[] { SearchDocType.Node, SearchDocType.File }).Any(t => docTypes.Any(d => d == t));

            try
            {
                List<SearchDoc> retDocs = new List<SearchDoc>();
                List<SearchDoc> toBeRemoved = new List<SearchDoc>();

                Query query = null;
                IndexSearcher searcher = null;

                search(applicationId, ref retDocs, ref toBeRemoved, currentUserId, ref lowerBoundary, count - retDocs.Count,
                    ref query, ref searcher, phrase, docTypes, typeIds, types, additionalId, title, description, content,
                    tags, fileContent, forceHasContent, highlight, ref totalCount);

                if (searcher != null) searcher.Dispose();

                if (toBeRemoved.Count > 0) remove_docs(applicationId, toBeRemoved);

                return retDocs;
            }
            catch (Exception ex)
            {
                LogController.save_error_log(applicationId, null, "InitSearchIndexDocuments", ex, ModuleIdentifier.SRCH, LogLevel.Fatal);
                return new List<SearchDoc>();
            }
        }

        private static Dictionary<SearchDocType, List<Guid>> get_existing_ids(Guid applicationId,
            List<SearchDoc> docs, ref List<DocFileInfo> files)
        {
            Dictionary<SearchDocType, List<Guid>> ids = new Dictionary<SearchDocType, List<Guid>>();

            ids[SearchDocType.Node] = CNController.get_existing_node_ids(applicationId,
                docs.Where(u => u.SearchDocType == SearchDocType.Node).Select(v => v.ID).ToList(), true, false);

            ids[SearchDocType.NodeType] = CNController.get_existing_node_type_ids(applicationId,
                docs.Where(u => u.SearchDocType == SearchDocType.NodeType).Select(v => v.ID).ToList(), false);

            ids[SearchDocType.Question] = QAController.get_existing_question_ids(applicationId,
                docs.Where(u => u.SearchDocType == SearchDocType.Question).Select(v => v.ID).ToList());

            ids[SearchDocType.User] = UsersController.get_approved_user_ids(applicationId,
                docs.Where(u => u.SearchDocType == SearchDocType.User).Select(v => v.ID).ToList());

            //Files
            List<DocFileInfo> newFiles = DocumentsController.get_file_owner_nodes(applicationId,
                docs.Where(u => u.SearchDocType == SearchDocType.File).Select(v => v.ID).ToList())
                .Where(u => u.FileID.HasValue).ToList();
            ids[SearchDocType.File] = newFiles.Select(v => v.FileID.Value).ToList();

            foreach (DocFileInfo f in newFiles)
                if (!files.Any(u => u.FileID == f.FileID)) files.Add(f);
            //end of Files

            return ids;
        }

        private static void _update_index(Guid applicationId, List<SearchDoc> docs)
        {
            _remove_docs(applicationId, docs);
            _create_index(applicationId, docs);
        }

        private static void update_index(Guid applicationId, SearchDocType docType)
        {
            List<SearchDoc> docs = new List<SearchDoc>();

            _remove_all_docs(applicationId);

            if (docType == SearchDocType.Node)
            {
                List<Node> nodes = CNController.get_nodes(applicationId: applicationId);

                foreach (Node nd in nodes)
                    docs.Add(new SearchDoc()
                    {
                        ID = nd.NodeID.Value,
                        AdditionalID = nd.AdditionalID,
                        Title = nd.Name,
                        Description = nd.Description,
                        Type = SearchDocType.Node.ToString()
                    });
            }

            _create_index(applicationId, docs);
        }

        private static void update_index(Guid applicationId, List<SearchDoc> docs)
        {
            if (RaaiVanSettings.Solr.Enabled)
            {
                SolrAPI.add(applicationId, docs);
                return;
            }

            _update_index(applicationId, docs);
        }

        private static void remove_docs(Guid applicationId, List<SearchDoc> docs)
        {
            _remove_docs(applicationId, docs);
        }

        public static void start_update(object rvThread)
        {
            RVJob trd = (RVJob)rvThread;

            if (!trd.TenantID.HasValue || !RaaiVanSettings.IndexUpdate.Index(trd.TenantID.Value)) return;

            if (!trd.TenantID.HasValue) return;

            if (!trd.StartTime.HasValue) trd.StartTime = RaaiVanSettings.IndexUpdate.StartTime(trd.TenantID.Value);
            if (!trd.EndTime.HasValue) trd.EndTime = RaaiVanSettings.IndexUpdate.EndTime(trd.TenantID.Value);

            while (true)
            {
                //sleep thread be madate Interval saniye
                if (!trd.Interval.HasValue) trd.Interval = RaaiVanSettings.IndexUpdate.Interval(trd.TenantID.Value);
                else Thread.Sleep(trd.Interval.Value);

                //agar dar saati hastim ke bayad update shavad edame midahim
                if (!trd.check_time()) continue;

                System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
                sw.Start();

                try
                {
                    //Aya Index ha dar RAM ham zakhire shavand
                    bool inRam = RaaiVanSettings.IndexUpdate.Ram(trd.TenantID.Value);
                    
                    //tartibe index kardane Search Doc ha cheghoone bashad
                    for (int i = 0; i < RaaiVanSettings.IndexUpdate.Priorities(trd.TenantID.Value).Length; i++)
                    {
                        SearchDocType type = SearchDocType.All;
                        if (!Enum.TryParse(RaaiVanSettings.IndexUpdate.Priorities(trd.TenantID.Value)[i], out type))
                            type = SearchDocType.All;

                        update_index(trd.TenantID.Value, type, RaaiVanSettings.IndexUpdate.BatchSize(trd.TenantID.Value), 20000);

                        Thread.Sleep(20000);
                    }
                }
                catch (Exception ex)
                {
                    LogController.save_error_log(trd.TenantID.Value, null, "IndexUpdateJob", ex, ModuleIdentifier.SRCH, LogLevel.Fatal);
                }

                trd.LastActivityDate = DateTime.Now;

                sw.Stop();
                trd.LastActivityDuration = sw.ElapsedMilliseconds;
            }
        }

        public static void update_index(Guid applicationId, SearchDocType type, int batchSize, int sleepInterval = 20000) {
            //Update Tags Before Index Update: Because tagextraction uses IndexLastUpdateDate field
            if (type == SearchDocType.Node) CNController.update_form_and_wiki_tags(applicationId, batchSize);

            List<SearchDoc> updateSdList = SearchController.get_index_queue_items(applicationId, batchSize, type);

            List<SearchDoc> deletedSdList = updateSdList.Where(u => u.Deleted == true).ToList();
            List<Guid> IDs = updateSdList.Select(u => u.ID).ToList();

            deletedSdList.ForEach(sd => updateSdList.Remove(sd));

            SearchUtilities.remove_docs(applicationId, deletedSdList);

            if(!RaaiVanSettings.Solr.Enabled) Thread.Sleep(sleepInterval);

            SearchUtilities.update_index(applicationId, updateSdList);

            SearchController.set_index_last_update_date(applicationId, type, IDs);
        }
    }
}