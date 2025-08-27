/*
* MIT License
*
* Copyright (c) 2025 [AUTHOR OR ORGANIZATION]
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/
package org.ecews.renaissance.web;

import jakarta.validation.Valid;
import lombok.RequiredArgsConstructor;
import org.ecews.renaissance.dtos.TriageDto;
import org.ecews.renaissance.services.TriageService;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;
import java.util.UUID;

@RestController
@RequestMapping("/api/ren/triage")
@RequiredArgsConstructor
public class TriageController {
    
    private static final Logger log = LoggerFactory.getLogger(TriageController.class);
    private final TriageService triageService;
    
    /**
     * Get first 10 triages (non-archived)
     */
    @GetMapping
    public ResponseEntity<List<TriageDto>> getAllTriages() {
        return ResponseEntity.ok(triageService.getAllTriages());
    }
    
    /**
     * Get a single triage by ID
     */
    @GetMapping("/{id}")
    public ResponseEntity<TriageDto> getTriage(@PathVariable UUID id) {
        return ResponseEntity.ok(triageService.getTriageById(id));
    }
    
    /**
     * Create a new triage
     */
    @PostMapping
    public ResponseEntity<TriageDto> createTriage(@Valid @RequestBody TriageDto triageDto) {
        log.info("From controller ***** {}", triageDto);
        return ResponseEntity.ok(triageService.createTriage(triageDto));
    }
    
    /**
     * Update an existing triage
     */
    @PutMapping("/{id}")
    public ResponseEntity<TriageDto> updateTriage(@PathVariable UUID id, @Valid @RequestBody TriageDto triageDto) {
        return ResponseEntity.ok(triageService.updateTriage(id, triageDto));
    }
    
    /**
     * Soft delete (archive) a triage
     */
    @DeleteMapping("/{id}")
    public ResponseEntity<Void> archiveTriage(@PathVariable UUID id) {
        triageService.archiveTriage(id);
        return ResponseEntity.noContent().build();
    }
    
    @GetMapping("/patient/{patientId}")
    public ResponseEntity<List<TriageDto>> getTriagesByPatientId(@PathVariable UUID patientId) {
        return ResponseEntity.ok(triageService.getTriagesByPatientId(patientId));
    }
    
}
