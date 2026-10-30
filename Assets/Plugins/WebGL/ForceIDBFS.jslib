mergeInto(LibraryManager.library, {
  ForceIDBFSLink__deps: ['$IDBFS'],
  ForceIDBFSLink: function () {
    // No-op. The dependency declaration forces Emscripten to link IDBFS.
  }
});
