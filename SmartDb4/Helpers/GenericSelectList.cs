using System;
using System.Linq;
using System.Web.Mvc;
using SmartDb4.DAL;
using SmartDb4.Enums;

namespace SmartDb4.Helpers
{
    public static class GenericSelectList
    {
        public static SelectList GetSelectList(SelectListEnums type, string dataValueField, string dataTextField, object selectedValue = null)
        {
            using (var uow = new UnitOfWork())
            {
                switch (type)
                {
                    case SelectListEnums.User:
                        {
                            var data = uow.UserRepository.Get(filter: x => x.UserName.ToLower() != Constants.Superadmin, orderBy: x => x.OrderBy(k => k.FirstName));
                            return new SelectList(data, dataValueField, dataTextField, selectedValue);
                        }
                    case SelectListEnums.UserType:
                        {
                            var data = uow.UserTypeRepository.Get(orderBy: x => x.OrderBy(k => k.UserTypeName));
                            return new SelectList(data, dataValueField, dataTextField, selectedValue);
                        }
                    case SelectListEnums.Classification:
                        {
                            var data = uow.ClassificationRepository.Get(orderBy: x => x.OrderBy(k => k.ClassificationName));
                            return new SelectList(data, dataValueField, dataTextField, selectedValue);
                        }
                    case SelectListEnums.Roles:
                        {
                            var data = uow.RoleRepository.Get(orderBy: x => x.OrderBy(k => k.MemberRoleName));
                            return new SelectList(data, dataValueField, dataTextField, selectedValue);
                        }
                    case SelectListEnums.Gender:
                        {
                            var data = uow.GenderRepository.Get(orderBy: x => x.OrderBy(k => k.GenderName));
                            return new SelectList(data, "GenderId", dataTextField, selectedValue);
                        }
                    case SelectListEnums.SexualOrientation:
                        {
                            var data = uow.SexualOrientationRepository.Get(orderBy: x => x.OrderBy(k => k.SexualOrientationName));
                            return new SelectList(data, "SexualOrientationId", dataTextField, selectedValue);
                        }
                    case SelectListEnums.LivingArea:
                        {
                            var data = uow.LivingAreaRepository.Get(orderBy: x => x.OrderBy(k => k.LivingAreaName));
                            return new SelectList(data, dataValueField, dataTextField, selectedValue);
                        }
                    case SelectListEnums.Ethnicity:
                        {
                            var data = uow.EthinicityRepository.Get(orderBy: x => x.OrderBy(k => k.EthnicityName));
                            return new SelectList(data, dataValueField, dataTextField, selectedValue);
                        }
                    case SelectListEnums.Nomination:
                        {
                            var data = uow.NominationRepository.Get(orderBy: x => x.OrderBy(k => k.NominationName));
                            return new SelectList(data, dataValueField, dataTextField, selectedValue);
                        }
                    case SelectListEnums.MemberRole:
                        {
                            var data = uow.MemberRoleRepository.Get(orderBy: x => x.OrderBy(k => k.MemberRoleName));
                            return new SelectList(data, dataValueField, dataTextField, selectedValue);
                        }
                    case SelectListEnums.FundingResponsibility:
                        {
                            var data = uow.FundingResponsibilityRepository.Get(orderBy: x => x.OrderBy(k => k.FundingResponsibilityName));
                            return new SelectList(data, dataValueField, dataTextField, selectedValue);
                        }
                    case SelectListEnums.Project:
                        {
                            var data = uow.ProjectRepository.Get(orderBy: x => x.OrderBy(k => k.ProjectName));
                            return new SelectList(data, dataValueField, dataTextField, selectedValue);
                        }
                    case SelectListEnums.GroupBy:
                        {
                            var data = uow.GroupByRepository.Get(orderBy: x => x.OrderBy(k => k.SortOrder));
                            return new SelectList(data, dataValueField, dataTextField, selectedValue);
                        }
                    case SelectListEnums.AgeBracket:
                        {
                            var data = uow.AgeBracketRepository.Get(orderBy: x => x.OrderBy(k => k.AgeBracketText));
                            return new SelectList(data, dataValueField, dataTextField, selectedValue);
                        }
                    case SelectListEnums.ReferralType:
                        {
                            var data = uow.ReferralTypeRepository.Get(orderBy: x => x.OrderBy(k => k.ReferralTypeName));
                            return new SelectList(data, "ReferralTypeId", dataTextField, selectedValue);
                        }
                    default:
                        throw new ArgumentOutOfRangeException("type");
                }
            }
        }
    }
}