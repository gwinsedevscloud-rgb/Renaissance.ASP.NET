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

import lombok.RequiredArgsConstructor;
import org.ecews.renaissance.domain.Optometrist;
import org.ecews.renaissance.dtos.OptometristDTO;
import org.ecews.renaissance.services.OptometristService;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;
import java.util.UUID;

@RestController
@RequestMapping("/api/optometrists")
@RequiredArgsConstructor
public class OptometristController {
    
    private final OptometristService optometristService;
    
    @PostMapping
    public ResponseEntity<Optometrist> save(@RequestBody OptometristDTO dto) {
        return ResponseEntity.ok(optometristService.save(dto));
    }
    
    @PutMapping("/{id}")
    public ResponseEntity<Optometrist> update(@PathVariable UUID id, @RequestBody OptometristDTO dto) {
        return ResponseEntity.ok(optometristService.update(id, dto));
    }
    
    @DeleteMapping("/{id}")
    public ResponseEntity<Void> delete(@PathVariable UUID id) {
        optometristService.delete(id);
        return ResponseEntity.noContent().build();
    }
    
    @GetMapping("/{id}")
    public ResponseEntity<Optometrist> findById(@PathVariable UUID id) {
        return ResponseEntity.ok(optometristService.findById(id));
    }
    
    @GetMapping("/by-patient/{patientId}")
    public ResponseEntity<List<Optometrist>> findByPatient(@PathVariable UUID patientId) {
        return ResponseEntity.ok(optometristService.findByPatientId(patientId));
    }
    
    @GetMapping
    public ResponseEntity<List<Optometrist>> findAll() {
        return ResponseEntity.ok(optometristService.findAll());
    }
}
