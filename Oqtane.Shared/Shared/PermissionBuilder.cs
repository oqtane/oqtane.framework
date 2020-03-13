using System.Collections.Generic;

namespace Oqtane.Shared
{
    public class PermissionBuilder
    {
        private readonly List<PermissionModel> _model = new List<PermissionModel>();

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

            public PermissionModel GrantTo(string role)
            {
                Subjects.Add(new Subject {Deny = false, Name = role});
                return this;
            }

            public PermissionModel DenyTo(string role)
            {
                Subjects.Add(new Subject {Deny = true, Name = role});
                return this;
            }

            public override string ToString()
            {
                var subjects = string.Join(";", Subjects);
                return $"{{\"PermissionName\":\"{Permission}\",\"Permissions\":\"{subjects}\"}}";
            }
        }

        public class Subject
        {
            private PermissionModel Parent { get; set; }
            public string Name { get; set; }
            public bool Deny { get; set; }

            public override string ToString()
            {
                return Deny ? "!" + Name : Name;
            }
        }

        public PermissionModel Permit(Permissions permission)
        {
            var p = new PermissionModel(this)
            {
                Permission = permission,
            };
            _model.Add(p);
            return p;
        }

// "[{\"PermissionName\":\"View\",\"Permissions\":\"All Users;Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]"
        public override string ToString()
        {
            string result = "[";
            result += string.Join(",", _model);
            return result + "]";
        }
    }
}
