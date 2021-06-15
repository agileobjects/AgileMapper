using System;
using AgileObjects.AgileMapper;
using AgileObjects.AgileMapper.ObjectPopulation;
using AgileObjects.AgileMapper.UnitTests.Common.TestClasses;
using AgileObjects.ReadableExpressions;
using AgileObjects.ReadableExpressions.Extensions;

namespace AgileObjects.AgileMapper.Buildable.UnitTests.Mappers
{
    public class StringMapper : MappingExecutionContextBase<string>
    {
        public StringMapper
        (
            string source
        )
        : base(source)
        {
        }

        public TTarget ToANew<TTarget>()
        {
            if (typeof(TTarget) == typeof(Title))
            {
                return (TTarget)((object)StringMapper.CreateNew(this.CreateRootMappingData(default(Title))));
            }

            throw new NotSupportedException(
                "Unable to perform a 'CreateNew' mapping from source type 'string' to target type '" + typeof(TTarget).GetFriendlyName(null) + "'");
        }

        private static Title CreateNew
        (
            IObjectMappingData<string, Title> sToTData
        )
        {
            try
            {
                if (sToTData.Source == null)
                {
                    return default(Title);
                }

                switch (sToTData.Source.ToUpperInvariant())
                {
                    case "0":
                    case "UNKNOWN":
                        return Title.Unknown;

                    case "1":
                    case "MR":
                        return Title.Mr;

                    case "2":
                    case "MASTER":
                        return Title.Master;

                    case "3":
                    case "MS":
                        return Title.Ms;

                    case "4":
                    case "MISS":
                        return Title.Miss;

                    case "5":
                    case "MRS":
                        return Title.Mrs;

                    case "6":
                    case "DR":
                        return Title.Dr;

                    case "7":
                    case "HON":
                        return Title.Hon;

                    case "8":
                    case "DUKE":
                        return Title.Duke;

                    case "9":
                    case "COUNT":
                        return Title.Count;

                    case "10":
                    case "EARL":
                        return Title.Earl;

                    case "11":
                    case "VISCOUNT":
                        return Title.Viscount;

                    case "12":
                    case "LORD":
                        return Title.Lord;

                    case "13":
                    case "LADY":
                        return Title.Lady;

                    case "14":
                    case "OTHER":
                        return Title.Other;
                }

                return default(Title);
            }
            catch (Exception ex)
            {
                throw MappingException.For(
                    "CreateNew",
                    "string",
                    "Title",
                    ex);
            }
        }
    }
}