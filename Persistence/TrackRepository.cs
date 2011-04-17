using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence
{
    class TrackRepository
    {
        private Repository<TrackModel.Track> _repository;
        public TrackRepository(string repType,RepositoryConfiguration conf) {
            this._repository = RepositoryFactory.GetRepositoryInstance<TrackModel.Track>(repType, conf);
        }
        public RepositoryResponse InsertTrack(string filename) {
            if (System.IO.File.Exists(filename)) {
                TrackModel tk=new TrackModel(filename);
                RepositoryResponse rsp=this._repository.Save(tk);
                if (rsp>=0) {
                    return RepositoryResponse.RepositoryInsert;
                } else {
                    return rsp;
                }
            } else if(System.IO.Directory.Exists(filename)) {
                string[] files=System.IO.Directory.GetFiles(filename,"*.mp3");
                Parallel.ForEach(files,file => {
                    TrackModel tk = new TrackModel(file);
                    this._repository.Save(tk);
                });
                return RepositoryResponse.RepositoryInsert;
            } else {
                return RepositoryResponse.RepositoryGenericError;
            }
        }
        public RepositoryResponse InsertTrack(TrackModel mdl) {
            RepositoryResponse rsp = this._repository.Save(mdl);
            if (rsp >= 0 ) {
                return RepositoryResponse.RepositoryInsert;
            } else {
                return rsp;
            }
        }
        public RepositoryResponse Delete(string key) {
            return this._repository.Delete(key);
        }
        public RepositoryResponse Update(TrackModel mdl) {
            return this._repository.Save(mdl);
        }
        public TrackModel Get(string key) {
            TrackModel tk = new TrackModel();
            if (this._repository.GetByKey(key, tk) >= 0)
            {
                return tk;
            }
            else
            {
                return null;
            }
        }
        public ICollection<TrackModel> GetAll() {
            LinkedList<TrackModel> list = new LinkedList<TrackModel>();
            LinkedList<TrackModel.Track> dbList= new LinkedList<TrackModel.Track>();
            RepositoryResponse rsp = this._repository.GetAll(dbList);
            Parallel.ForEach(dbList, elem =>
            {
                TrackModel tk = new TrackModel();
                tk.LoadFromDatabaseType(elem);
                list.AddLast(tk);
            });
            return list;
        }
        public int Count() {
            return this._repository.Count();
        }
    }
}// namespace Persistence
