namespace QarnotSDK {

    // This class is used to serialize constants in pools and tasks
    internal class KeyValHelper {
        public string Key { get; set; }

        public string Value { get; set; }

        public KeyValHelper(string key, string value) {
            Key = key;
            Value = value;
        }

        public KeyValHelper() {
        }
    }

    // This class is used to serialize errors
    internal class Error {
        public string Message { get; set; }

        public Error(string msg) {
            Message = msg;
        }

        public Error() {
        }
    }
}
