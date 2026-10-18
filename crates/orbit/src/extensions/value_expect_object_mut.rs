//! Mutable access to a JSON object by pointer in tests.

use crate::prelude::*;

/// Mutable access to a JSON object by pointer.
pub trait ExpectObjectMut {
    /// Object at the JSON `pointer`.
    ///
    /// # Panics
    ///
    /// - Panics when `pointer` does not resolve to an object
    fn expect_object_mut(&mut self, pointer: &str) -> &mut Map<String, Value>;
}

impl ExpectObjectMut for Value {
    fn expect_object_mut(&mut self, pointer: &str) -> &mut Map<String, Value> {
        self.pointer_mut(pointer)
            .and_then(Value::as_object_mut)
            .expect("pointer should resolve to an object")
    }
}
