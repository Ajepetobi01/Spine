using Spine.Common.Attributes;
using Spine.Common.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ManageSubcription.Api.Helpers
{
    public static class SortedRequestExtension
    {
        // <summary>
        /// Searches thru the model that will be used for the request for uses of SortableAttribute
        /// on its properties, to determine what columns will be used in the order by.
        /// </summary>
        /// <typeparam name="T">Model of the entity being requested</typeparam>
        /// <param name="request"></param>
        /// <param name="addUniqueSort">If the model's unique field should be added to the end of the sort string,
        ///     to confirm the order of result will not change.
        /// </param>
        /// <returns></returns>
        public static string FindSortingAndOrder<T>(this ISortedRequest request, bool addUniqueSort = false)
        {
            var resultSortBy = string.Empty;
            string uniqueSort = null;

            //If sort by was not sent in the request, then we need to look for the attribute that is specified as the default
            var findDefault = string.IsNullOrWhiteSpace(request.SortBy);
            foreach (var propertyInfo in typeof(T).GetProperties())
            {
                var sortable = propertyInfo.GetCustomAttribute<SortableAttribute>();
                if (sortable == null)
                {
                    continue;
                }

                if (addUniqueSort && sortable.IsUnique)
                {
                    //There can only be 1 field labeled as unique. If multiple are found, throw an error
                    if (uniqueSort != null)
                    {
                        throw new ValidationException();
                    }

                    uniqueSort = sortable.Columns;

                    //If the sort by column is already found, all that is still needed is to verify there is only 1 unique field
                    if (resultSortBy != string.Empty)
                    {
                        continue;
                    }
                }

                if (findDefault)
                {
                    if (sortable.IsDefault)
                    {
                        resultSortBy = sortable.Columns;
                        if (!addUniqueSort)
                        {
                            break;
                        }
                    }

                    //If we are looking for the default then there is no reason to continue to the matches check.
                    continue;
                }

                //A property will be used to sort, if the property name is
                //the same as the one passed in the request (case insensitive)
                if (propertyInfo.Name.Equals(request.SortBy, StringComparison.OrdinalIgnoreCase))
                {
                    resultSortBy = sortable.Columns;
                    if (!addUniqueSort)
                    {
                        break;
                    }
                }
            }

            if (addUniqueSort)
            {
                //If this model needs a unique field and none was found, throw an error
                if (uniqueSort == null)
                {
                    throw new ValidationException();
                }

                //Add the unique field to the result sort if it already has a value, otherwise sort only by the unique field
                resultSortBy += resultSortBy == string.Empty ? uniqueSort : "," + uniqueSort;
            }

            //The point of this is to put the order after each column that is being sorted
            //Ex: guest.LastName,guest.FirstName becomes guest.LastName asc,guest.FirstName asc
            return string.Join(",", resultSortBy.Split(',').Select(s => s + " " + request.Order));
        }
    }
}
