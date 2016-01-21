using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace SenkaKichi.DbModels
{
    public partial class SenkaData
    {
        public short? RankPointDeltaExtra {
            get {
                if (this.ExperienceDelta == null) {
                    return null;
                }

                double deltaDiff = this.RankPointDelta.Value - this.ExactRankPointDelta;
                if (deltaDiff < 10) {
                    return 0;
                }
                return (short)(Math.Round(deltaDiff / 5.0) * 5);
            }
        }

        public double ExactRankPointDelta {
            get {
                if (this.ExperienceDelta == null) {
                    return 0;
                }
                return this.ExperienceDelta.Value * 70 / 100000.0;
            }
        }

        public override string ToString() {
            return string.Format("Ranking: {0}, Name: {1}, Comment: {2}, RankPoint: {3}", 
                this.Ranking, this.Player.Name, this.Comment, this.RankPoint);
        }

        internal void SetDelta(SenkaData previousData, bool isFirstDay) {
            if (this.PlayerId != previousData.PlayerId) throw new IdNotMatchException();

            if (isFirstDay) {
                this.RankPointDelta = this.RankPoint;
            } else {
                this.RankingDelta = (short)(this.Ranking - previousData.Ranking);
                this.RankPointDelta = (short)(this.RankPoint - previousData.RankPoint);
                this.ExperienceDelta = this.Experience - previousData.Experience;
            }
        }

        internal void SetRankAllDelta(SenkaData previousData) {
            if (this.PlayerId != previousData.PlayerId) throw new IdNotMatchException();

            if (previousData.RankingAll.HasValue) {
                this.RankingAllDelta = (short)(this.RankingAll.Value - previousData.RankingAll.Value);
            }
        }

        internal void SetDelta() {
            this.RankPointDelta = this.RankPoint;
        }

        public string RankType {
            get {
                switch (this.RankTypeId) {
                    case 1:
                        return "元帥";
                    case 2:
                        return "大将";
                    case 3:
                        return "中将";
                    case 4:
                        return "少将";
                    case 5:
                        return "大佐";
                    case 6:
                        return "中佐";
                    case 7:
                        return "新米中佐";
                    case 8:
                        return "少佐";
                    case 9:
                        return "中堅少佐";
                    case 10:
                        return "新米少佐";
                    default:
                        return null;
                }
            }

        }
    }

    [Serializable]
    public class IdNotMatchException : Exception
    {
        public IdNotMatchException() : base("The PlayerId between two datas did not matched.") { }
        protected IdNotMatchException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}