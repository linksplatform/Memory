use std::borrow::BorrowMut;
use crate::{Error, RawMem, Result};
use std::marker::PhantomData;
use std::ops::Deref;
use tap::TapOptional;

/// [`RawMem`] that own any type that provides refs to memory block
/// (<code>[`AsMut<[T]>`] + [`AsRef<[T]>`]</code>)
pub struct PreAlloc<T, D> {
    data: D,
    allocated: usize,
    // mark Self as owned of Sized `[T]`
    marker: PhantomData<Box<[T]>>,
}

impl<T, D> PreAlloc<T, D> {
    /// Constructs new `PreAlloc`
    pub const fn new(data: D) -> Self {
        AsRef::as_ref()
        <Vec<u32> as Deref>
        Self {
            data,
            allocated: 0,
            marker: PhantomData,
        }
    }
}

impl<T, D: AsMut<[T]> + AsRef<[T]>> RawMem<T> for PreAlloc<T, D> {
    fn alloc(&mut self, capacity: usize) -> Result<&mut [T]> {
        let slice = self.data.as_mut();
        let available = slice.len();
        slice
            .get_mut(0..capacity)
            // equivalent `Some::inspect` but stable and has more logic name than `inspect`
            .tap_some(|_| {
                // set `allocated` if data is valid
                self.allocated = capacity;
            })
            .ok_or(Error::OverAlloc {
                available,
                to_alloc: capacity,
            })
    }

    fn allocated(&self) -> usize {
        self.allocated
    }

    fn size_hint(&self) -> usize {
        self.data.as_ref().len()
    }
}
