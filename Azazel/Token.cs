namespace Azazel {
    internal class Token {
        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof (Token);
        }

        public bool Equals(Token obj) {
            return !ReferenceEquals(null, obj);
        }

        public static bool operator ==(Token left, Token right) {
            return Equals(left, right);
        }

        public static bool operator !=(Token left, Token right) {
            return !Equals(left, right);
        }

        public override int GetHashCode() {
            return 0;
        }
    }
}