use h3o::{CellIndex, LatLng, Resolution};

// --- helpers -----------------------------------------------------------------

fn to_cell(raw: u64) -> Option<CellIndex> {
    CellIndex::try_from(raw).ok()
}

fn to_res(raw: i32) -> Option<Resolution> {
    Resolution::try_from(raw as u8).ok()
}

// --- cell lookup -------------------------------------------------------------

#[unsafe(no_mangle)]
pub extern "C" fn geogrid_cell_at(lat_deg: f64, lng_deg: f64, res: i32) -> u64 {
    let Some(resolution) = to_res(res) else { return u64::MAX };
    let coord = LatLng::new(lat_deg, lng_deg).expect("valid lat/lng");
    u64::from(coord.to_cell(resolution))
}

// --- cell introspection ------------------------------------------------------

#[unsafe(no_mangle)]
pub extern "C" fn geogrid_resolution_of(cell: u64) -> i32 {
    to_cell(cell).map(|c| u8::from(c.resolution()) as i32).unwrap_or(-1)
}

#[unsafe(no_mangle)]
pub extern "C" fn geogrid_is_valid(cell: u64) -> bool {
    to_cell(cell).is_some()
}

// --- geometry ----------------------------------------------------------------

/// Writes the cell centre into `out_lat` / `out_lng`. Returns false on invalid cell.
#[unsafe(no_mangle)]
pub extern "C" fn geogrid_center_of(cell: u64, out_lat: *mut f64, out_lng: *mut f64) -> bool {
    let Some(c) = to_cell(cell) else { return false };
    let ll = LatLng::from(c);
    unsafe {
        *out_lat = ll.lat();
        *out_lng = ll.lng();
    }
    true
}

/// Writes boundary vertices into `out` (caller allocates 10 × 2 f64s: [lat0, lng0, lat1, lng1, …]).
/// Returns the actual vertex count, or -1 on invalid cell.
#[unsafe(no_mangle)]
pub extern "C" fn geogrid_boundary_of(cell: u64, out: *mut f64) -> i32 {
    let Some(c) = to_cell(cell) else { return -1 };
    let boundary = c.boundary();
    let verts: Vec<LatLng> = boundary.iter().copied().collect();
    unsafe {
        for (i, ll) in verts.iter().enumerate() {
            *out.add(i * 2) = ll.lat();
            *out.add(i * 2 + 1) = ll.lng();
        }
    }
    verts.len() as i32
}

// --- hierarchy ---------------------------------------------------------------

#[unsafe(no_mangle)]
pub extern "C" fn geogrid_parent_of(cell: u64, parent_res: i32) -> u64 {
    let Some(c) = to_cell(cell) else { return u64::MAX };
    let Some(r) = to_res(parent_res) else { return u64::MAX };
    c.parent(r).map(u64::from).unwrap_or(u64::MAX)
}

#[unsafe(no_mangle)]
pub extern "C" fn geogrid_children_len(cell: u64, child_res: i32) -> i32 {
    let Some(c) = to_cell(cell) else { return -1 };
    let Some(r) = to_res(child_res) else { return -1 };
    c.children_count(r) as i32
}

#[unsafe(no_mangle)]
pub extern "C" fn geogrid_children_of(cell: u64, child_res: i32, out: *mut u64, len: i32) -> i32 {
    let Some(c) = to_cell(cell) else { return -1 };
    let Some(r) = to_res(child_res) else { return -1 };
    let children: Vec<CellIndex> = c.children(r).collect();
    let count = children.len().min(len as usize);
    unsafe {
        for (i, child) in children.iter().take(count).enumerate() {
            *out.add(i) = u64::from(*child);
        }
    }
    count as i32
}

// --- root cells --------------------------------------------------------------

#[unsafe(no_mangle)]
pub extern "C" fn geogrid_root_cells_len() -> i32 {
    CellIndex::base_cells().count() as i32
}

#[unsafe(no_mangle)]
pub extern "C" fn geogrid_root_cells(out: *mut u64, len: i32) {
    let cells: Vec<CellIndex> = CellIndex::base_cells().collect();
    let count = cells.len().min(len as usize);
    unsafe {
        for (i, c) in cells.iter().take(count).enumerate() {
            *out.add(i) = u64::from(*c);
        }
    }
}

// --- cells at resolution -----------------------------------------------------

#[unsafe(no_mangle)]
pub extern "C" fn geogrid_cells_at_resolution_len(res: i32) -> i32 {
    let Some(r) = to_res(res) else { return -1 };
    CellIndex::base_cells()
        .flat_map(move |base| base.children(r))
        .count() as i32
}

#[unsafe(no_mangle)]
pub extern "C" fn geogrid_cells_at_resolution(res: i32, out: *mut u64, len: i32) -> i32 {
    let Some(r) = to_res(res) else { return -1 };
    let mut count = 0;
    unsafe {
        for cell in CellIndex::base_cells().flat_map(move |base| base.children(r)) {
            if count >= len as usize { break; }
            *out.add(count) = u64::from(cell);
            count += 1;
        }
    }
    count as i32
}

// --- disk --------------------------------------------------------------------

#[unsafe(no_mangle)]
pub extern "C" fn geogrid_disk_len(cell: u64, k: i32) -> i32 {
    let Some(c) = to_cell(cell) else { return -1 };
    c.grid_disk::<Vec<_>>(k as u32).len() as i32
}

#[unsafe(no_mangle)]
pub extern "C" fn geogrid_disk(cell: u64, k: i32, out: *mut u64, len: i32) -> i32 {
    let Some(c) = to_cell(cell) else { return -1 };
    let disk: Vec<CellIndex> = c.grid_disk(k as u32);
    let count = disk.len().min(len as usize);
    unsafe {
        for (i, d) in disk.iter().take(count).enumerate() {
            *out.add(i) = u64::from(*d);
        }
    }
    count as i32
}
