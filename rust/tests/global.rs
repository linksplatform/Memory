#![feature(allocator_api)]

use platform_mem::{GlobalMem, RawMem};
use std::error::Error;

#[test]
fn basic() -> Result<(), Box<dyn Error>> {
    let mut mem = GlobalMem::<usize>::new();
    let slice = mem.alloc(10)?;
    assert_eq!(slice.len(), 10);

    slice.iter_mut().enumerate().for_each(|(i, x)| *x = i);

    assert_eq!(slice, [0, 1, 2, 3, 4, 5, 6, 7, 8, 9]);

    let slice = mem.alloc(20)?;
    assert_eq!(slice.len(), 20);
    slice.iter_mut().enumerate().for_each(|(i, x)| *x = i);

    assert_eq!(
        slice,
        [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19]
    );
    Ok(())
}

#[test]
fn with_non_default_inner() -> Result<(), Box<dyn Error>> {
    let mut mem = GlobalMem::<String>::new();
    let slice = mem.alloc(10)?;

    slice
        .iter_mut()
        .enumerate()
        .for_each(|(i, x)| *x = i.to_string());

    assert_eq!(slice, ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9"]);

    let slice = mem.alloc(20)?;
    slice
        .iter_mut()
        .enumerate()
        .for_each(|(i, x)| *x = i.to_string());

    assert_eq!(
        slice,
        [
            "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15",
            "16", "17", "18", "19"
        ]
    );
    Ok(())
}
