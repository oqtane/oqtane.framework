using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.Json;
using Oqtane.Models;

namespace Oqtane.Shared
{
    public class PermissionBuilder
    {
        private readonly Dictionary<Permissions, PermissionModel> _model = new Dictionary<Permissions, PermissionModel>();

        public static PermissionBuilder Parse(string permissionString)
        {
            var pb = new PermissionBuilder();
            if (!string.IsNullOrEmpty(permissionString))
            {
                var ps = JsonSerializer.Deserialize<List<PermissionString>>(permissionString);
                foreach (var s in ps)
                {
                    if (Enum.TryParse(s.PermissionName, out Permissions permission))
                    {
                        var subjects = s.Permissions.Split(";", StringSplitOptions.RemoveEmptyEntries);
                        foreach (var subject in subjects)
                        {
                            var pm = pb.Permit(permission);
                            if (subject.StartsWith("!"))
                            {
                                pm.DenyTo(subject);
                            }
                            else
                            {
                                pm.GrantTo(subject);
                            }
                        }
                    }
                }
            }

            return pb;
        }

        public bool IsAuthorized(User user, Permissions permission)
        {
            if (!_model.TryGetValue(permission, out PermissionModel model)) return false;
            var roles = user != null ? $"[{user.UserId}];{user.Roles}".Split(";", StringSplitOptions.RemoveEmptyEntries) : new[] {Constants.AllUsersRole};
            return model.IsAuthorized(roles);
        }

        public PermissionModel Permit(Permissions permission)
        {
            if (_model.ContainsKey(permission))
            {
                return _model[permission];
            }

            var p = new PermissionModel(this)
            {
                Permission = permission,
            };
            _model.Add(permission, p);
            return p;
        }

        public override string ToString()
        {
            string result = "[";
            result += string.Join(",", _model.Select(x => x.Value));
            return result + "]";
        }

        public class PermissionModel
        {
            internal PermissionModel(PermissionBuilder parent)
            {
                Parent = parent;
            }

            private PermissionBuilder Parent { get; set; }
            public Permissions Permission { get; set; }
            private List<Subject> Subjects { get; set; } = new List<Subject>();

            public PermissionModel Permit(Permissions permission)
            {
                return Parent.Permit(permission);
            }

            internal bool IsAuthorized(string[] roles)
            {
                var result = Subjects
                    .Where(x => roles.Contains(x.Name))
                    .Select(x => !x.Deny).ToList();
                return result.Any() && result.Aggregate((x, y) => x && y);
            }


            public PermissionModel GrantTo(string role)
            {
                Set(role, false);
                return this;
            }

            private void Set(string role, bool deny)
            {
                var subject = Subjects.FirstOrDefault(s => s.Name == role);
                if (subject != null)
                {
                    subject.Deny = deny;
                    return;
                }

                Subjects.Add(new Subject {Deny = deny, Name = role});
            }

            public PermissionModel DenyTo(string role)
            {
                Set(role, true);
                return this;
            }

            public override string ToString()
            {
                if (!Subjects.Any()) return null;
                var subjects = string.Join(";", Subjects);
                return $"{{\"PermissionName\":\"{Permission}\",\"Permissions\":\"{subjects}\"}}";
            }
        }

        private class Subject
        {
            public string Name { get; set; }
            public bool Deny { get; set; }

            public override string ToString()
            {
                return Deny ? "!" + Name : Name;
            }
        }
    }
}
